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
    public class Scope
    {
        Extents3d ext;
        public List<InsBuilding> Buildings { get; set; }        

        public Scope (Extents3d ext, List<InsBuilding> items)
        {            
            this.ext = ext;
            Buildings = items;
        }
    }
}
