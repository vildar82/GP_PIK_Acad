using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.Insolation.Models;

namespace PIK_GP_Acad.Insolation.Services
{
    /// <summary>
    /// Визуализация площадки
    /// </summary>
    public class VisualPlace : VisualDatabase
    {
        private List<Entity> visuals;
        public VisualPlace (Document doc) : base (doc)
        {

        }

        public List<Tile> Tiles { get { return tiles; } set { tiles = value; UnionTiles(); } }
        List<Tile> tiles;        

        public override List<Entity> CreateVisual ()
        {
            return visuals;
        }

        private void UnionTiles ()
        {
            var groupLevels = Tiles.GroupBy(g => g.Level);
            foreach (var group in groupLevels)
            {
                var pls = group.Select(s => s.Contour).ToList();
                var region = pls.Union(null);                
            }
        }
    }
}
