using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.Insolation.Models;

namespace PIK_GP_Acad.Insolation.Services
{
    /// <summary>
    /// Визуализация контуров площадки
    /// </summary>
    public class VisualPlaceContours : VisualDatabase
    {
        PlaceModel placeModel;
        List<Entity> visuals;
        public VisualPlaceContours(PlaceModel placeModel) : base(placeModel.Model.Doc)
        {
            this.placeModel = placeModel;
        }

        public override List<Entity> CreateVisual()
        {
            return visuals?.Select(s => (Entity)s.Clone()).ToList();
        }

        public void DisposeVisuals()
        {
            if (visuals == null) return;
            foreach (var item in visuals)
            {
                item?.Dispose();
            }
            visuals = null;         
        }

        public void AddRegion(Region region)
        {
            if (visuals == null)
                visuals = new List<Entity>();
            visuals.Add(region);
        }

        public override void Dispose()
        {
            DisposeVisuals();
            base.Dispose();
        }
    }
}
