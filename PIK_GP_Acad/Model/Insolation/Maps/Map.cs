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
using PIK_GP_Acad.Insolation.Options;
using PIK_GP_Acad.Model.Insolation.ShadowMap;

namespace PIK_GP_Acad.Insolation
{    
    /// <summary>
    /// карта - чертеж с объектами расчета инсоляции
    /// </summary>
    public class Map : IMap
    {
        public Document Doc { get; set; }
        List<InsBuilding> buildings;
        Database db;        
        RTree<InsBuilding> treeBuildings;
        RTree<Tile> treeTiles;
        /// <summary>
        /// Ячейки карты
        /// </summary>
        public List<Tile> Tiles { get; set; }        

        public Map(Document doc)
        {
            this.Doc = doc;
            this.db = doc.Database;            
            LoadMap();            
            // TODO: подписаться на события изменения объектов чертежа - чтобы отслеживать изменения карты
        }

        /// <summary>
        /// Определение объектов инсоляции в чертеже
        /// </summary>
        private void LoadMap ()
        {
            FCS.FCService.Init(db);
            buildings = new List<InsBuilding>();
            treeBuildings = new RTree<InsBuilding>();
            using (var t = db.TransactionManager.StartTransaction())
            {
                var ms = db.CurrentSpaceId.GetObject(OpenMode.ForRead) as BlockTableRecord;
                foreach (var idEnt in ms)
                {
                    var ent = idEnt.GetObject(OpenMode.ForRead) as Entity;
                    var building = ElementFactory.Create<IBuilding>(ent);
                    if (building != null)
                    {
                        var insBuild = new InsBuilding(building);
                        buildings.Add(insBuild);                        
                        treeBuildings.Add(new Rectangle(building.ExtentsInModel), insBuild);
                    }
                }
                t.Commit();
            }           
        }

        /// <summary>
        /// Определение расчетной области и объектов в ней
        /// </summary>        
        public Scope GetScope (Extents3d ext)
        {            
            Rectangle rectScope = new Rectangle(ext);
            var items = treeBuildings.Intersects(rectScope);
            Scope scope = new Scope(ext, items);
            return scope;
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

        private void CreateTiles (int tileSize)
        {
            Tiles = new List<Tile>();
            treeTiles = new RTree<Tile>();
            Tile.Size = tileSize;
            Rectangle boundsBuildings = treeBuildings.getBounds();

            double len = boundsBuildings.max[0] - boundsBuildings.min[0];
            double width = boundsBuildings.max[1] - boundsBuildings.min[1];

            int countTilesInLen = Convert.ToInt32(len / tileSize) + tileSize;
            int countTilesInWidth = Convert.ToInt32(width / tileSize) + tileSize;

            double tileHalfSize = tileSize * 0.5;

            double x;
            double y = boundsBuildings.min[1];
            for (int w = 0; w < countTilesInWidth; w++)
            {
                y += tileSize;
                x = boundsBuildings.min[0];
                for (int l = 0; l < countTilesInLen; l++)
                {
                    x += tileSize;
                    Point3d center = new Point3d(x, y, 0);
                    Tile tile = new Tile(center);
                    Tiles.Add(tile);

                    // добавление в дерево
                    var r = new Rectangle(x - tileHalfSize, y - tileHalfSize, x + tileHalfSize, y + tileHalfSize, 0, 0);
                    treeTiles.Add(r, tile);
                }
            }            
        }        
    }
}
