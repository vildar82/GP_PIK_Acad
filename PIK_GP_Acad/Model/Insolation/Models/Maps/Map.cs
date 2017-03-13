using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using AcadLib.RTree.SpatialIndex;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.Elements;
using PIK_GP_Acad.Elements.Buildings;
using AcadLib.XData;
using PIK_GP_Acad.Insolation.Services;
using AcadLib.Errors;
using System.Diagnostics;
using AcadLib.Extensions;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// карта - чертеж с объектами расчета инсоляции
    /// </summary>
    public class Map : IDisposable
    {
        Database db;
        RTree<MapBuilding> treeBuildings;
        //VisualsMap visualMap;
        
        //RTree<Tile> treeTiles;
        //public List<Tile> Tiles { get; set; }                

        public Map()
        {            
            Doc = Application.DocumentManager.MdiActiveDocument;
            db = Doc.Database;
            IsVisualOn = false;
        }

        public Map(InsModel insModel)
        {
            Model = insModel;
            IsVisualOn = true; // По-умолчаию визуализация включена
            Doc = insModel.Doc;
            db = Doc.Database;            
        }

        public InsModel Model { get; set; }
        public bool IsEventsOn { get; set; }        
        public Document Doc { get; set; }
        public double MaxBuildingHeight => GetMaxBuildingHeight();
        public List<MapBuilding> Buildings { get; private set; }
        /// <summary>
        /// Найденные точки инсоляции
        /// </summary>
        public List<ObjectId> InsPoints { get; private set; }
        public List<KeyValuePair<ObjectId, DicED>> Places { get; private set; }
        /// <summary>
        /// Включение/ отключение визуализации карты при обновлениях
        /// </summary>
        public bool IsVisualOn { get; set; }
        /// <summary>
        /// Определение домов при загрузке капрты
        /// </summary>
        public bool IsDefineHousesWhenLoadMap { get; set; } = true;
        public HouseMap Houses { get; set; }

        /// <summary>
        /// Добавлено здание
        /// </summary>
        public event EventHandler<MapBuilding> BuildingAdded;
        /// <summary>
        /// Здание удалено
        /// </summary>
        public event EventHandler<MapBuilding> BuildingErased;
        /// <summary>
        /// Здание изменилось (удалено и создаено новое)
        /// Передается старое здание
        /// </summary>
        public event EventHandler<MapBuilding> BuildingModified;
        /// <summary>
        /// Добавлена расчетная точка
        /// </summary>
        public event EventHandler<ObjectId> InsPointAdded;
        
        /// <summary>
        /// Определение объектов инсоляции в чертеже
        /// </summary>
        public void LoadMap()
        {
            IsEventsOn = false;
            FCS.FCService.Init(db);
            MapBuilding.IndexesCounter = 0;
            Buildings = new List<MapBuilding>();
            InsPoints = new List<ObjectId>();
            Places = new List<KeyValuePair<ObjectId, DicED>>();
            treeBuildings = new RTree<MapBuilding>();
            using (Doc.LockDocument())
            using (var t = db.TransactionManager.StartTransaction())
            {
                var ms = db.CurrentSpaceId.GetObject(OpenMode.ForRead) as BlockTableRecord;
                foreach (var idEnt in ms)
                {
                    if (idEnt.IsValidEx())
                    {
                        var ent = idEnt.GetObject(OpenMode.ForRead, false, true) as Entity;
                        DefineEnt(ent);
                    }
                }

                if (IsDefineHousesWhenLoadMap)
                {
                    Houses = new HouseMap(this);
                    Houses.DefineHouses();
                }

                t.Commit();
            }
            IsEventsOn = true;

            Debug.WriteLine($"На карте определено: зданий {Buildings.Count}, точек {InsPoints.Count}, площадок {Places.Count}");
        }

        public void Update()
        {            
            Unsubscribe();
            //ClearVisual();
            Dispose();
            LoadMap();
            SubscribeDB();
            //UpdateVisual();

            if (Model?.Options?.EnableCheckDublicates ?? true)
            {
                try
                {
                    CheckBuildingIntersect.Check(this);
                }
                catch (Exception ex)
                {
                    Logger.Log.Error(ex, "Insolation.Map.CheckBuildingIntersect.Check().");
                }
            }
        }

        public void UpdateVisual()
        {
            if (Buildings == null) return;
            if (IsVisualOn)
            {
                foreach (var item in Buildings)
                {
                    if (item.House?.FrontGroup == null)
                        item.UpdateVisual(); 
                }
                //if (visualMap == null)
                //{
                //    visualMap = new VisualsMap(this);
                //}
                //visualMap.VisualIsOn = true;
            }
            else
            {
                foreach (var item in Buildings)
                {
                    item?.DisposeVisual();
                }
                //visualMap?.Dispose();
                //visualMap = null;
            }
        }

        private double GetMaxBuildingHeight()
        {
            double res = 0;
            if (Buildings.Count != 0)
            {
                res = Buildings.Max(b => b.HeightCalc);
            }
            return res;
        }

        private void SubscribeDB()
        {
            try
            {
                db.ObjectAppended -= Database_ObjectAppended; // перестраховка
                db.ObjectAppended += Database_ObjectAppended;
            }
            catch
            {
                // ignored
            }
        }

        public void Unsubscribe()
        {
            try
            {
                db.ObjectAppended -= Database_ObjectAppended;
            }
            catch { }
            if (Buildings == null) return;
            using (var t = db.TransactionManager.StartTransaction())
            {
                foreach (var item in Buildings)
                {
                    if (item.Building.IdEnt.IsValidEx())
                    {
                        var dbo = item.Building.IdEnt.GetObject(OpenMode.ForRead) as Entity;
                        try
                        {
                            dbo.Modified -= Building_Modified;
                            dbo.Erased -= Building_Erased;
                        }
                        catch
                        {
                            break;
                        }
                    }
                }
                t.Commit();
            }
        }

        private void DefineEnt(Entity ent)
        {
            if (ent == null) return;
            IBuilding building = null;            
            try
            {
                building = ElementFactory.Create<IBuilding>(ent);
            }
            catch(Exception ex)
            {
                Inspector.AddError($"Ошибка при определении здания - {ex.Message}", ent);
                return;
            }

            if (building != null && building.IsVisible)
            {                
                // Если это не полилиния или блок - такие объекты пока не поддерживаются
                if (!(ent is Polyline) && !(ent is BlockReference))
                {
                    Inspector.AddError($"Тип здания не подходит для инсоляции - '{ent.GetRXClass().Name}', слой '{ent.Layer}'", ent, System.Drawing.SystemIcons.Error);
                    return;
                }

                var insBuild = new MapBuilding(building);
                Buildings.Add(insBuild);
                var r = GetBuildingRectangle(insBuild);
                treeBuildings.Add(r, insBuild);

                // Подписывание на изменения объекта                
                ent.Modified += Building_Modified;
                ent.Erased += Building_Erased;

                // Оповещение расчета о изменении здания   
                if (IsEventsOn)
                    BuildingAdded?.Invoke(this, insBuild);
                return;
            }
            // Сбор точек инсоляции
            var dbPt = ent as DBPoint;
            if (dbPt != null)
            {
                if (InsPointHelper.IsInsPoint(dbPt))
                {
                    InsPoints.Add(dbPt.Id);

                    // Оповещение о создании точки
                    if (IsEventsOn)
                        InsPointAdded?.Invoke(this, dbPt.Id);
                }
                return;
            }
            var pl = ent as Polyline;
            if (pl != null)
            {                
                var entExtDicExt = new EntDictExt(pl, InsService.PluginName);
                var dic = entExtDicExt.Load();
                if (dic != null)
                {
                    var dicPlace = dic.GetInner(Place.PlaceDicName);
                    if (dicPlace != null)
                    {
                        Places.Add(new KeyValuePair<ObjectId, DicED>(pl.Id, dicPlace));
                    }
                }
                //// Если она на  слое sapr_ins_visuals - удаление
                //if (VisualDatabase.IsVisualElement(ent))
                //{
                //    ent.UpgradeOpen();
                //    ent.Erase();
                //}
            }
        }

        private Rectangle GetBuildingRectangle(MapBuilding building)
        {
#if TEST
            //EntityHelper.AddEntityToCurrentSpace(building.ExtentsInModel.GetPolyline());
#endif
            return new Rectangle(building.ExtentsInModel);
        }

        /// <summary>
        /// Определение расчетной области и объектов в ней
        /// </summary>        
        public Scope GetScope(Extents3d ext)
        {
            var rectScope = new Rectangle(ext);
            var items = treeBuildings.Intersects(rectScope);
            var scope = new Scope(items, this);
            scope.InitBuildingContours();
            return scope;
        }

        public List<MapBuilding> GetBuildingsInExtents(Extents3d ext)
        {
            var rectScope = new Rectangle(ext);
            return treeBuildings.Intersects(rectScope);
        }

        public MapBuilding GetBuildingInPoint(Point2d pt)
        {
            MapBuilding building = null;
            var buildingsInPt = GetBuildingsInPoint(pt);
            if (buildingsInPt.Count == 1)
            {
                building = buildingsInPt[0];
            }
            else if (buildingsInPt.Count > 1)
            {
                // Выбрать ближайшую полилинию                
                double minLen = -1;
                var pt3d = pt.Convert3d();
                foreach (var buildItem in buildingsInPt)
                {
                    bool initContour = false;
                    if (buildItem.Contour == null || buildItem.Contour.IsDisposed)
                    {
                        buildItem.InitContour();
                        initContour = true;
                    }

                    try
                    {
                        var ptContour = buildItem.Contour.GetClosestPointTo(pt3d, false);
                        var l = (pt3d - ptContour).Length;
                        if (l < minLen || minLen == -1)
                        {
                            minLen = l;
                            building = buildItem;
                        }
                    }
                    catch { }

                    if (initContour)
                        buildItem.Contour.Dispose();
                }
            }
            return building;
        }

        public List<MapBuilding> GetBuildingsInPoint(Point2d pt)
        {
            Point p = new Point(pt.X, pt.Y, 0);
            var nearest = treeBuildings.Nearest(p, 2);
            return nearest;
        }

        public MapBuilding GetBuildingInPoint(Point3d pt)
        {
            return GetBuildingInPoint(pt.Convert2d());
        }

        private MapBuilding FindBuildingByEnt(ObjectId id)
        {
            return Buildings.Find(b => b.Building.IdEnt == id);
        }

        /// <summary>
        /// Добавление объекта в чертеж
        /// </summary>        
        private void Database_ObjectAppended(object sender, ObjectEventArgs e)
        {
            //var ent = e.DBObject as Entity;
            //if (ent == null) return;
            //try
            //{
            //    DefineEnt(ent);
            //}
            //catch { }
        }

        /// <summary>
        /// Удаление здания
        /// </summary>        
        private void Building_Erased(object sender, ObjectErasedEventArgs e)
        {
            // Определение удаленного здания
            var building = FindBuildingByEnt(e.DBObject.Id);
            if (building != null)
            {                
                Buildings.Remove(building);
                building.Dispose();
                var r = GetBuildingRectangle(building);
                treeBuildings.Delete(r, building);

                if (IsEventsOn)
                    BuildingErased?.Invoke(this, building);
            }
        }

        /// <summary>
        /// Изменение здания (перемещение)
        /// </summary>        
        private void Building_Modified(object sender, EventArgs e)
        {
            var ent = sender as Entity;
            if (ent == null) return;

            //// Поиск старого здания
            //var buildingOld = FindBuildingByEnt(ent.Id);
            //if (buildingOld == null) return;
            //// удаление старого здания из списка и создание нового
            //Buildings.Remove(buildingOld);
            //treeBuildings.Delete(GetBuildingRectangle(buildingOld), buildingOld);

            //IBuilding buildingNew;
            //using (var t = ent.Database.TransactionManager.StartTransaction())
            //{
            //    buildingNew = ElementFactory.Create<IBuilding>(ent);
            //    t.Commit();
            //}            
            //if (buildingNew != null)
            //{
            //    var insBuildNew = new InsBuilding(buildingNew);
            //    Buildings.Add(insBuildNew);
            //    var r = GetBuildingRectangle(insBuildNew);
            //    treeBuildings.Add(r, insBuildNew);                

            //    if (IsEventsOn)
            //        BuildingModified?.Invoke(this, buildingOld);
            //}
            if (IsEventsOn)
                BuildingModified?.Invoke(this, null);
        }

        /// <summary>
        /// Очистка визуализаций и отписка от событий
        /// </summary>
        public void ClearVisual()
        {
            // отписатся от всех событий
            // Удалить всю визуализацию
            //visualMap?.VisualsDelete();
            if (Buildings != null)
                foreach (var item in Buildings)
                {
                    item.Visual?.VisualsDelete();
                }
            Unsubscribe();
        }

        public void Dispose()
        {
            if (db == null || db.IsDisposed) return;
            Houses?.Dispose();
            
            //visualMap?.Dispose();
            Unsubscribe();
            if (Buildings != null)
            {
                foreach (var build in Buildings)
                {
                    build?.Dispose();
                }
            }
        }

        /// <summary>
        /// Вывести статистику определенных объектов на карте
        /// </summary>
        public void WriteReport()
        {
            var ed = Doc.Editor;
            ed.WriteMessage($"\nОпределенно объектов инсоляции на чертеже:");
            // Здания
            var groupsBuildings = Buildings.GroupBy(g => g.Building.IsProjectedBuilding).Select(s=> {
                if (s.Key)
                {
                    // Проектируемые
                    return $"Проектируемые-{s.Count()}шт.";
                }
                else
                {
                    // Окружающие
                    return $"Окр.застройка-{s.Count()}шт.";
                }
            });
            ed.WriteMessage($"\nЗданий: {Buildings?.Count ?? 0} шт. {string.Join(", ",groupsBuildings)}");
            // Точек
            ed.WriteMessage($"\nРасчетных точек: {InsPoints?.Count ?? 0} шт.");
            // Площадок
            ed.WriteMessage($"\nПлощадок: {Places?.Count ?? 0} шт.");
        }
    }
}