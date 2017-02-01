using PIK_GP_Acad.Insolation.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Визуализация карты
    /// </summary>
    public class VisualsMap : VisualTransient
    {
        private Map map;

        public VisualsMap (Map map)
        {
            this.map = map;                        
        }

        public override List<Entity> CreateVisual()
        {
            if (map == null) return null;
            var visuals = new List<Entity>();
            if (map.Buildings != null)
            {
                foreach (var item in map.Buildings)
                {
                    var draws = item.CreateVisual();
                    if (draws != null && draws.Any())
                    {
                        visuals.AddRange(draws);
                    }
                }                
            }            
            return visuals;
        }
    }
}
