﻿using System;
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
    public class VisualPlace : VisualTransient
    {
        private List<Entity> visuals;
        private PlaceModel placeModel;
        /// <summary>
        /// Визуализация контуров уровней площадок на чертеже
        /// </summary>
        private VisualPlaceContours visualPlaceContours;
        public VisualPlace (PlaceModel placeModel)// : base (placeModel?.Model?.Doc)
        {
            this.placeModel = placeModel;
            visualPlaceContours = new VisualPlaceContours(placeModel);
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
            visualPlaceContours.VisualIsOn = this.VisualIsOn;
            return visuals?.Select(s=>(Entity)s.Clone()).ToList();
        }

        private void UnionTiles ()
        {
            if (Tiles == null || Tiles.Count == 0) return;

            DisposeVisuals();
            visualPlaceContours.DisposeVisuals();
            visuals = new List<Entity>();

            var groupLevels = Tiles.GroupBy(g => g.Level);
            foreach (var group in groupLevels)
            {
                if (group.Key.TotalTimeH == 0) continue;
                var pls = group.Select(s => s.Contour).ToList();

                var region = pls.Union(null);

                // Добавление региона в визуализацию контуров уровней площадок
                var visOpt = new VisualOption(group.Key.Color, placeModel.Options.Transparent);
                VisualHelper.SetEntityOpt(region, visOpt);                
                visualPlaceContours.AddRegion(region);                

                var h = region.CreateHatch();                
                VisualHelper.SetEntityOpt(h, visOpt);
                visuals.Add(h);
            }
        }

        public override void VisualsDelete()
        {
            visualPlaceContours.VisualsDelete();
            base.VisualsDelete();
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
            visualPlaceContours.Dispose();
            base.Dispose();
        }
    }
}
