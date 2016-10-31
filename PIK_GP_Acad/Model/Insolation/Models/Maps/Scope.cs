using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.Elements;
using PIK_GP_Acad.Elements.Buildings;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Расчетная область
    /// </summary>
    public class Scope : IDisposable
    {
        Extents3d ext;
        public List<MapBuilding> Buildings { get; set; }        

        public Scope (Extents3d ext, List<MapBuilding> items)
        {            
            this.ext = ext;
            Buildings = items;
        }

        public void Dispose ()
        {
            foreach (var item in Buildings)
            {
                if (item.Contour != null || !item.Contour.IsDisposed)
                    item.Contour.Dispose();
            }
        }
    }
}
