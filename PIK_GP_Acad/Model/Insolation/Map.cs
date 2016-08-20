using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.RTree.SpatialIndex;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.Elements;
using PIK_GP_Acad.Elements.Buildings;

namespace PIK_GP_Acad.Insolation
{    
    /// <summary>
    /// карта - чертеж с объектами расчета инсоляции
    /// </summary>
    public class Map
    {
        private List<IBuilding> buildings;
        Database db;
        Options options;
        RTree<IBuilding> treeBuildings;
        public Map(Database db, Options options)
        {
            this.db = db;
            this.options = options;
            LoadMap();
        }

        /// <summary>
        /// Определение объектов инсоляции в чертеже
        /// </summary>
        private void LoadMap ()
        {
            FCS.FCService.Init(db);
            buildings = new List<IBuilding>();
            treeBuildings = new RTree<IBuilding>();
            var ms = db.CurrentSpaceId.GetObject(OpenMode.ForRead) as BlockTableRecord;
            foreach (var idEnt in ms)
            {
                var ent = idEnt.GetObject(OpenMode.ForRead) as Entity;
                var building = ElementFactory.Create<IBuilding>(ent);
                if (building != null)
                {
                    buildings.Add(building);
                    Extents3d ext = building.ExtentsInModel;                    
                    treeBuildings.Add(new Rectangle(ext), building);
                }
            }            
        }

        /// <summary>
        /// Определение расчетной области и объектов в ней
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public Scope GetScopeInPoint (Point3d pt)
        {
            int maxHeight = options.MaxHeight;
            Extents3d ext = options.SunlightRule.GetScanExtents (pt, maxHeight);            
            Rectangle rectScope = new Rectangle(ext);
            var items = treeBuildings.Intersects(rectScope);
            Scope scope = new Scope (ext, items);
            return scope;
        }
    }
}
