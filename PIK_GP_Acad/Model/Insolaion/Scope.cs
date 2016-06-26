using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.Insolaion.Constructions;

namespace PIK_GP_Acad.Insolaion
{
    /// <summary>
    /// Расчетная область
    /// </summary>
    public class Scope
    {
        Extents3d ext;
        List<IBuilding> items;
        public int Radius { get; set; }

        public Scope (int radius, Extents3d ext, List<IBuilding> items)
        {
            Radius = radius;
            this.ext = ext;
            this.items = items;
        }
    }
}
