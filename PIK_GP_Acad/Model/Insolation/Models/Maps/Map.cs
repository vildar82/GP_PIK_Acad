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

namespace PIK_GP_Acad.Insolation.Models
{    
    /// <summary>
    /// карта - чертеж с объектами расчета инсоляции
    /// </summary>
    public class Map : IDisposable
    {   
        Database db;
        RTree<MapBuilding> treeBuildings;
        //RTree<Tile> treeTiles;
        //public List<Tile> Tiles { get; set; }                

        public Map (Database db)
        {   
            this.db = db;
            LoadMap();
            SubscribeDB();
        }

        public bool IsEventsOn { get; set; }
        public Document Doc { get; set; }
        public int MaxBuildingHeight => GetMaxBuildingHeight();
        public List<MapBuilding> Buildings { get; private set; }       
        /// <summary>
        /// Найденные точки инсоляции
        /// </summary>
        public List<ObjectId> InsPoints { get; private set; }
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
        private void LoadMap ()
        {
            IsEventsOn = false;
            FCS.FCService.Init(db);
            Buildings = new List<MapBuilding>();
            InsPoints = new List<ObjectId>();
            treeBuildings = new RTree<MapBuilding>();
            using (var t = db.TransactionManager.StartTransaction())
            {
                var ms = db.CurrentSpaceId.GetObject(OpenMode.ForRead) as BlockTableRecord;
                foreach (var idEnt in ms)
                {
                    if (idEnt.IsValidEx())
                    {
                        var ent = idEnt.GetObject(OpenMode.ForRead) as Entity;
                        DefineEnt(ent);
                    }
                }
                t.Commit();
            }            
            IsEventsOn = true;
        }

        public void Update ()
        {
            Clear();            
            LoadMap();
            SubscribeDB();
        }

        private int GetMaxBuildingHeight ()
        {
            int res = 0;
            if (Buildings.Count != 0)
            {
                res = Buildings.Max(b => b.Height);
            }
            return res;
        }

        private void SubscribeDB ()
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

        private void Unsubscribe ()
        {
            db.ObjectAppended -= Database_ObjectAppended;
            using (var t = db.TransactionManager.StartTransaction())
            {
                foreach (var item in Buildings)
                {
                    var dbo = item.Building.IdEnt.GetObject(OpenMode.ForRead);
                    try
                    {
                        dbo.Modified -= Building_Modified;
                        dbo.Erased -= Building_Erased;
                    }
                    catch { }
                }
                t.Commit();
            }
        }

        private void DefineEnt (Entity ent)
        {
            var building = ElementFactory.Create<IBuilding>(ent);
            if (building != null)
            {
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
            }
            // Сбор точек инсоляции
            else if (ent is DBPoint)
            {
                var dbPt = (DBPoint)ent;
                if (InsPointHelper.IsInsPoint(dbPt))
                {
                    InsPoints.Add(dbPt.Id);

                    // Оповещение о создании точки
                    if (IsEventsOn)
                        InsPointAdded?.Invoke(this, dbPt.Id);
                }
            }
        }

        private Rectangle GetBuildingRectangle (MapBuilding building)
        {
            return new Rectangle(building.ExtentsInModel);
        }

        /// <summary>
        /// Определение расчетной области и объектов в ней
        /// </summary>        
        public Scope GetScope (Extents3d ext)
        {            
            var rectScope = new Rectangle(ext);
            var items = treeBuildings.Intersects(rectScope);
            var scope = new Scope(ext, items, this);
            scope.InitContour();
            return scope;
        }

        public MapBuilding GetBuildingInPoint (Point2d pt)
        {
            MapBuilding building = null;
            Point p = new Point(pt.X, pt.Y, 0);
            var nearest = treeBuildings.Nearest(p, 2);
            if (nearest.Count > 0)
            {
                building = nearest[0];
            }
            return building;
        }

        public MapBuilding GetBuildingInPoint (Point3d pt)
        {
            return GetBuildingInPoint(pt.Convert2d());            
        }

        private MapBuilding FindBuildingByEnt (ObjectId id)
        {
            return Buildings.Find(b => b.Building.IdEnt == id);
        }

        /// <summary>
        /// Добавление объекта в чертеж
        /// </summary>        
        private void Database_ObjectAppended (object sender, ObjectEventArgs e)
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
        private void Building_Erased (object sender, ObjectErasedEventArgs e)
        {
            // Определение удаленного здания
            MapBuilding building = FindBuildingByEnt(e.DBObject.Id);
            if (building != null)
            {                
                Buildings.Remove(building);
                var r = GetBuildingRectangle(building);
                treeBuildings.Delete(r, building);

                if (IsEventsOn)
                    BuildingErased?.Invoke(this, building);
            }
        }        

        /// <summary>
        /// Изменение здания (перемещение)
        /// </summary>        
        private void Building_Modified (object sender, EventArgs e)
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
        /// Очистка карты - отключение
        /// Транзация уже запущена
        /// </summary>
        public void Clear ()
        {
            // отписатся от всех событий
            // Удалить всю визуализацию (пока нет)
            Unsubscribe();
            
        }

        public void Dispose ()
        {
            if (db == null || db.IsDisposed) return;
            Unsubscribe();
        }
    }
}