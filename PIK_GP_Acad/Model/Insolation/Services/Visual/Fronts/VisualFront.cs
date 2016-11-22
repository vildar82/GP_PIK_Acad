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
    public class VisualFront : VisualDatabase, IDisposable
    {
        public VisualFront(Document doc): base(doc)
        {

        }

        public List<FrontValue> FrontLines { get; set; }

        public override List<Entity> CreateVisual ()
        {
            if (FrontLines == null) return null;
            return FrontLines.Select(s => (Polyline)s.Line.Clone()).Cast<Entity>().ToList();
        }

        public override void Dispose ()
        {
            if (FrontLines != null)
            {
                foreach (var item in FrontLines)
                {
                    if (item.Line != null && !item.Line.IsDisposed)
                    {
                        item.Line.Dispose();
                    }
                }
            }
            base.Dispose();
        }
    }
}
