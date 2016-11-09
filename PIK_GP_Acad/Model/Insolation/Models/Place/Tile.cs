using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Ячейка площадки
    /// </summary>
    public class Tile
    {
        public Tile (Point2d pt, double area, double size)
        {
            Point = pt;
            Area = area;
            Size = size;
            Contour = CreateContour();
        }        

        public double Area { get; set; }
        public double Size { get; set; }
        public Polyline Contour { get; set; }
        public InsValue InsValue { get; set; }
        public TileLevel Level { get; set; }
        public Point2d Point { get; set; }

        private Polyline CreateContour ()
        {
            var pl = new Polyline();
            var pt = new Point2d(Point.X- Size*0.5, Point.Y-Size*0.5);
            pl.AddVertexAt(0, pt,0,0,0);

            pt = new Point2d(pt.X, pt.Y + Size);
            pl.AddVertexAt(1, pt, 0, 0, 0);

            pt = new Point2d(pt.X+Size, pt.Y);
            pl.AddVertexAt(2, pt, 0, 0, 0);

            pt = new Point2d(pt.X, pt.Y-Size);
            pl.AddVertexAt(3, pt, 0, 0, 0);

            pl.Closed = true;
            return pl;
        }
    }
}
