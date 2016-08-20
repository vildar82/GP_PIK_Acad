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
using PIK_GP_Acad.Model.Insolation.Shadow;

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
        /// <summary>
        /// Ячейки карты
        /// </summary>
        public List<Tile> Tiles { get; set; }
        RTree<Tile> treeTiles;

        public Map(Database db, Options options)
        {
            this.db = db;
            this.options = options;
            LoadMap();
            CreateTiles();
        }

        /// <summary>
        /// Определение объектов инсоляции в чертеже
        /// </summary>
        private void LoadMap ()
        {
            FCS.FCService.Init(db);
            buildings = new List<IBuilding>();
            treeBuildings = new RTree<IBuilding>();
            using (var t = db.TransactionManager.StartTransaction())
            {
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
                t.Commit();
            }           
        }        

        private void CreateTiles ()
        {
            Tiles = new List<Tile>();
            treeTiles = new RTree<Tile>();
            Tile.Size = options.TileSize;
            Rectangle boundsBuildings = treeBuildings.getBounds();

            double len = boundsBuildings.max[0] - boundsBuildings.min[0];
            double width = boundsBuildings.max[1] - boundsBuildings.min[1];

            int countTilesInLen = Convert.ToInt32(len / options.TileSize) + options.TileSize;
            int countTilesInWidth = Convert.ToInt32(width / options.TileSize) + options.TileSize;

            double tileHalfSize = options.TileSize * 0.5;

            double x;
            double y = boundsBuildings.min[1];
            for (int w = 0; w < countTilesInWidth; w++)
            {
                y += options.TileSize;
                x = boundsBuildings.min[0];
                for (int l = 0; l < countTilesInLen; l++)
                {
                    x += options.TileSize;
                    Point3d center = new Point3d(x, y, 0);
                    Tile tile = new Tile(center);
                    Tiles.Add(tile);

                    // добавление в дерево
                    var r = new Rectangle(x - tileHalfSize, y - tileHalfSize, x + tileHalfSize, y + tileHalfSize, 0, 0);
                    treeTiles.Add(r, tile);
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
            Extents3d ext = options.SunlightRule.GetScanExtents(pt, maxHeight);
            Rectangle rectScope = new Rectangle(ext);
            var items = treeBuildings.Intersects(rectScope);
            Scope scope = new Scope(ext, items);
            return scope;
        }
    }
}
