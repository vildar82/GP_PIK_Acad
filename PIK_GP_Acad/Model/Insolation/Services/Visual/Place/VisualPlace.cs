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
        private PlaceModel placeModel;
        public VisualPlace (PlaceModel placeModel) : base (placeModel?.Model?.Doc)
        {
            this.placeModel = placeModel;
        }

        public List<Tile> Tiles { get { return tiles; }
            set {
                DisposeTiles();
                tiles = value;
                UnionTiles();
            }
        }
        List<Tile> tiles;        

        public override List<Entity> CreateVisual ()
        {
            return visuals;
        }

        private void UnionTiles ()
        {
            if (Tiles == null || Tiles.Count == 0) return;

            DisposeVisuals();
            visuals = new List<Entity>();

            var groupLevels = Tiles.GroupBy(g => g.Level);
            foreach (var group in groupLevels)
            {
                if (group.Key.TotalTimeH == 0) return;
                var pls = group.Select(s => s.Contour).ToList();
                using (var region = pls.Union(null))
                {
                    var h = region.CreateHatch();
                    var visOpt = new VisualOption(group.Key.Color, placeModel.Options.Transparent);
                    VisualHelper.SetEntityOpt(h, visOpt);
                    visuals.Add(h);
                }
            }
        }

        private void DisposeVisuals ()
        {
            if (visuals != null)
            {
                foreach (var item in visuals)
                {
                    item.Dispose();
                }
            }
        }

        public void DisposeTiles ()
        {
            if (Tiles != null)
            {
                foreach (var item in Tiles)
                {
                    item.Contour?.Dispose();
                }
            }
        }
    }
}
