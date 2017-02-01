using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using AcadLib.Geometry;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.Insolation.Models;

namespace PIK_GP_Acad.Insolation.Services
{
    /// <summary>
    /// Визуализация площадки
    /// </summary>
    public class VisualPlace : VisualTransient
    {
        private List<Entity> visuals;
        private Place place;
                
        public VisualPlace (Place place) : base("ins_sapr_place")
        {
            this.place = place;            
        }

        public List<Tile> Tiles { get { return tiles; }
            set {                
                tiles = value;
                UnionTiles();
                DisposeTiles();
            }
        }
        List<Tile> tiles;

        public override List<Entity> CreateVisual ()
        {            
            return visuals?.Select(s=>(Entity)s.Clone()).ToList();
        }

        private void UnionTiles ()
        {
            if (Tiles == null || Tiles.Count == 0) return;

            DisposeVisuals();            
            visuals = new List<Entity>();

            // Подпись площадки
            var text = GetPlaceNameText();
            if (text != null)
                visuals.Add(text);

            var groupLevels = Tiles.GroupBy(g => g.Level);
            try
            {
                foreach (var group in groupLevels)
                {
                    if (group.Key.TotalTimeH == 0) continue;
                    var pls = group.Select(s => s.Contour).ToList();

                    var region = pls.Union(null);

                    // Добавление региона в визуализацию контуров уровней площадок
                    var visOpt = new VisualOption(group.Key.Color, place?.PlaceModel?.Options?.Transparent ?? 0);
                    VisualHelper.SetEntityOpt(region, visOpt);
                    visuals.Add(region);

                    var h = region.CreateHatch();
                    VisualHelper.SetEntityOpt(h, visOpt);
                    visuals.Add(h);
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "VisualPlace.UnionTiles()");
                DisposeVisuals();
            }
        }

        /// <summary>
        /// Тект имени площадки
        /// </summary>
        /// <returns></returns>
        private DBText GetPlaceNameText()
        {
            if (string.IsNullOrEmpty(place?.Name)) return null;
            var visOpt = new VisualOption(System.Drawing.Color.White);
            using (var pl = place.PlaceId.Open(OpenMode.ForRead) as Polyline)
            {
                try
                {
                    visOpt.Position = pl.Centroid();
                }
                catch
                {
                    return null;
                }
            }
            return VisualHelper.CreateText(place.Name, visOpt, 1, AttachmentPoint.MiddleCenter);
        }

        private void DisposeVisuals()
        {
            if (visuals == null) return;
            foreach (var item in visuals)
            {
                item.Dispose();
            }
            visuals = null;
        }

        private void DisposeTiles ()
        {
            if (Tiles != null)
            {
                foreach (var item in Tiles)
                {
                    item.Contour?.Dispose();
                }
            }
        }

        public override void Dispose()
        {
            DisposeTiles();
            DisposeVisuals();            
            base.Dispose();
        }
    }
}
