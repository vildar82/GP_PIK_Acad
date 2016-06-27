using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.RTree.SpatialIndex;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.Insolation.Constructions;

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
        RTree<IBuilding> rtree;
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
            buildings = new List<IBuilding>();
            rtree = new RTree<IBuilding>();
            var ms = db.CurrentSpaceId.GetObject( OpenMode.ForRead) as BlockTableRecord;
            foreach (var idEnt in ms)
            {
                var ent = idEnt.GetObject( OpenMode.ForRead, false, true) as Entity;
                var building = BuildingFactory.CreateBuilding(ent);
                if (building != null)
                {
                    buildings.Add(building);
                    Extents3d ext = building.ExtentsInModel;                    
                    rtree.Add(new Rectangle(ext), building);
                }
            }            
        }

        /// <summary>
        /// Определение расчетной области и объектов в ней
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public Scope GetScope (Point3d pt)
        {
            int maxHeight = options.MaxHeight;
            int side = options.SunlightRule.GetLength (maxHeight);
            Extents3d ext = new Extents3d (new Point3d(pt.X-side, pt.Y-side, 0), new Point3d (pt.X+side, pt.Y, 0));
            Rectangle rectScope = new Rectangle(ext);
            var items = rtree.Intersects(rectScope);
            Scope scope = new Scope (side, ext, items);
            return scope;
        }
    }
}
