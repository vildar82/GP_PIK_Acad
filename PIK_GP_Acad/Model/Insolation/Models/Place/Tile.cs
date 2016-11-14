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
        public Tile (Point2d pt, double width, double height)
        {
            Point = pt;
            Area = Math.Round(width*height,4);
            Width = width;
            Height = height;
            Contour = CreateContour();
        }        

        public double Area { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public Polyline Contour { get; set; }
        public InsValue InsValue { get; set; }
        public TileLevel Level { get; set; }
        public Point2d Point { get; set; }

        private Polyline CreateContour ()
        {
            var pl = new Polyline();
            var pt = new Point2d(Point.X- Width*0.5, Point.Y-Height*0.5);
            pl.AddVertexAt(0, pt,0,0,0);

            pt = new Point2d(pt.X, pt.Y + Height);
            pl.AddVertexAt(1, pt, 0, 0, 0);

            pt = new Point2d(pt.X+Width, pt.Y);
            pl.AddVertexAt(2, pt, 0, 0, 0);

            pt = new Point2d(pt.X, pt.Y-Height);
            pl.AddVertexAt(3, pt, 0, 0, 0);

            pl.Closed = true;
            return pl;
        }
    }
}
