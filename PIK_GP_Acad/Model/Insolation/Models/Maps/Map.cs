using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public class Map
    {        
        InsModel model;        
        Database db;
        RTree<InsBuilding> treeBuildings;
        //RTree<Tile> treeTiles;
        //public List<Tile> Tiles { get; set; }                

        public Map (InsModel model)
        {
            this.model = model;
            this.Doc = model.Doc;
            this.db = Doc.Database;
            LoadMap();
            // TODO: подписаться на события изменения объектов чертежа - чтобы отслеживать изменения карты
            Doc.Database.ObjectAppended += Database_ObjectAppended;
        }
        public bool IsEventsOn { get; set; }
        public Document Doc { get; set; }
        public int MaxBuildingHeight { get { return GetMaxBuildingHeight(); } }
        public List<InsBuilding> Buildings { get; private set; }       
        /// <summary>
        /// Найденные точки инсоляции
        /// </summary>
        public List<ObjectId> InsPoints { get; private set; }
        /// <summary>
        /// Добавлено здание
        /// </summary>
        public event EventHandler<InsBuilding> BuildingAdded;
        /// <summary>
        /// Здание удалено
        /// </summary>
        public event EventHandler<InsBuilding> BuildingErased;
        /// <summary>
        /// Здание изменилось (удалено и создаено новое)
        /// Передается старое здание
        /// </summary>
        public event EventHandler<InsBuilding> BuildingModified;        

        /// <summary>
        /// Добавлена расчетная точка
        /// </summary>
        public event EventHandler<ObjectId> InsPointAdded;
        public event EventHandler<InsBuilding> BuildingChangeType;
        /// <summary>
        /// Определение объектов инсоляции в чертеже
        /// </summary>
        private void LoadMap ()
        {
            IsEventsOn = false;
            FCS.FCService.Init(db);
            Buildings = new List<InsBuilding>();
            InsPoints = new List<ObjectId>();
            treeBuildings = new RTree<InsBuilding>();
            using (var t = db.TransactionManager.StartTransaction())
            {
                var ms = db.CurrentSpaceId.GetObject(OpenMode.ForRead) as BlockTableRecord;
                foreach (var idEnt in ms)
                {
                    var ent = idEnt.GetObject(OpenMode.ForRead) as Entity;
                    DefineEnt(ent);
                }
                t.Commit();
            }            
            IsEventsOn = true;
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

        private void DefineEnt (Entity ent)
        {
            var building = ElementFactory.Create<IBuilding>(ent);
            if (building != null)
            {
                var insBuild = new InsBuilding(building);
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

        private Rectangle GetBuildingRectangle (InsBuilding building)
        {
            return new Rectangle(building.ExtentsInModel);
        }

        /// <summary>
        /// Определение расчетной области и объектов в ней
        /// </summary>        
        public Scope GetScope (Extents3d ext)
        {            
            Rectangle rectScope = new Rectangle(ext);
            var items = treeBuildings.Intersects(rectScope);
            Scope scope = new Scope(ext, items);
            InitContour(scope.Buildings);
            return scope;
        }

        public void InitContour (List<InsBuilding> buildings)
        {
            foreach (var item in buildings)
            {
                item.InitContour();
            }
        }

        public InsBuilding GetBuildingInPoint (Point3d pt)
        {
            InsBuilding building = null;
            Point p = new Point(pt.X, pt.Y, 0);
            var nearest = treeBuildings.Nearest(p, 5);            
            if (nearest.Count ==1)
            {
                building = nearest[0];
            }
            return building;
        }

        private InsBuilding FindBuildingByEnt (ObjectId id)
        {
            return Buildings.Find(b => b.Building.IdEnt == id);
        }

        /// <summary>
        /// Добавление объекта в чертеж
        /// </summary>        
        private void Database_ObjectAppended (object sender, ObjectEventArgs e)
        {
            var ent = e.DBObject as Entity;
            if (ent == null) return;
            try
            {
                DefineEnt(ent);
            }
            catch { }
        }

        /// <summary>
        /// Удаление здания
        /// </summary>        
        private void Building_Erased (object sender, ObjectErasedEventArgs e)
        {
            // Определение удаленного здания
            InsBuilding building = FindBuildingByEnt(e.DBObject.Id);
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

            // Поиск старого здания
            var buildingOld = FindBuildingByEnt(ent.Id);
            if (buildingOld == null) return;
            // удаление старого здания из списка и создание нового
            Buildings.Remove(buildingOld);
            treeBuildings.Delete(GetBuildingRectangle(buildingOld), buildingOld);

            IBuilding buildingNew;
            using (var t = ent.Database.TransactionManager.StartTransaction())
            {
                buildingNew = ElementFactory.Create<IBuilding>(ent);
                t.Commit();
            }            
            if (buildingNew != null)
            {
                var insBuildNew = new InsBuilding(buildingNew);
                Buildings.Add(insBuildNew);
                var r = GetBuildingRectangle(insBuildNew);
                treeBuildings.Add(r, insBuildNew);                

                if (IsEventsOn)
                    BuildingModified?.Invoke(this, buildingOld);
            }            
        }

        /// <summary>
        /// Очистка карты - отключение
        /// Транзация уже запущена
        /// </summary>
        public void Clear ()
        {
            // отписатся от всех событий
            // Удалить всю визуализацию (пока нет)
            Doc.Database.ObjectAppended -= Database_ObjectAppended;
            foreach (var item in Buildings)
            {
                var dbo = item.Building.IdEnt.GetObject(OpenMode.ForRead);
                dbo.Modified -= Building_Modified;
                dbo.Erased -= Building_Erased;
            }
        }
    }
}
