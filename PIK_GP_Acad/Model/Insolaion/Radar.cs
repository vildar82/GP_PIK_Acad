using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.Insolaion.SunlightRule;

namespace PIK_GP_Acad.Insolaion
{
    /// <summary>
    /// Радар - расчета инсоляции в точке
    /// </summary>
    public class Radar
    {
        Options options;
        ISunlightRule rule;
        public Radar (Options options)
        {
            this.options = options;
            rule = options.SunlightRule;
        }

        /// <summary>
        /// Сканирование области из точки
        /// </summary>        
        public void Scan (Point3d pt, Scope scope)
        {
            // найти точки пересечения с объектами по направлению
            Point2d ptZero = new Point2d (pt.X, pt.Y);
            Vector2d vec = new Vector2d (ptZero.X+scope.Radius, ptZero.Y);
            Line2d ray = new Line2d (ptZero, vec);            
        }
    }
}
