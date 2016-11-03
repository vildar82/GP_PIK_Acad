using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.Insolation.Models;

namespace PIK_GP_Acad.Insolation.Services
{
    public class VisualFront : VisualTransient
    {
        public VisualFront()
        {

        }

        public List<FrontValue> FrontLines { get; set; }

        public override List<Entity> CreateVisual ()
        {
            if (FrontLines == null) return null;
            return FrontLines.Select(s => s.Line).Cast<Entity>().ToList();
        }        
    }
}
