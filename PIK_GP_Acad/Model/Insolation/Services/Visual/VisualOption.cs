using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace PIK_GP_Acad.Insolation.Services
{
    public class VisualOption
    {        
        public VisualOption(System.Drawing.Color c, Point3d pos, byte alpha=0)
        {
            SetColor(c);
            Position = pos;
            Transparency = new Transparency(alpha);            
        }

        public VisualOption(System.Drawing.Color c, byte alpha = 0)
        {
            SetColor(c);            
            Transparency = new Transparency(alpha);            
        }

        public Color Color { get; set; }
        public Transparency Transparency { get; set; }
        public Point3d Position { get; set; }             

        public void SetColor (System.Drawing.Color color)
        {
            Color = Color.FromColor(color);
        }
    }
}
