using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;

namespace PIK_GP_Acad.Model.Insolation.Shadow
{
    /// <summary>
    /// Ячейка карты - квадрат с координатой центра 
    /// </summary>
    public class Tile
    {
        public static int Size { get; set; }
        /// <summary>
        /// Точка центра ячейки
        /// </summary>
        Point3d Center { get; set; }

        Color Color { get; set; }       

        public Tile(Point3d center)
        {
            Center = center;
            Color = Color.FromColor(System.Drawing.Color.Red);
        }

        public Drawable CreateVisual ()
        {
            Autodesk.AutoCAD.DatabaseServices.Polyline pl = new Autodesk.AutoCAD.DatabaseServices.Polyline();
            pl.AddVertexAt(0, new Point2d(Center.X - Size * 0.45, Center.Y - Size * 0.45), 0, 0, 0);
            pl.AddVertexAt(1, new Point2d(Center.X - Size * 0.45, Center.Y + Size * 0.45), 0, 0, 0);
            pl.AddVertexAt(2, new Point2d(Center.X + Size * 0.45, Center.Y + Size * 0.45), 0, 0, 0);
            pl.AddVertexAt(3, new Point2d(Center.X + Size * 0.45, Center.Y - Size * 0.45), 0, 0, 0);
            pl.Closed = true;
            pl.Color = Color;
            return pl;


            //Hatch h = new Hatch();
            //h.SetHatchPattern(HatchPatternType.PreDefined, "SOLID");
            //h.Color = Color;

            //// Set our transparency to 25% (=127)
            //// Alpha value is Truncate(255 * (100-n)/100)
            //h.Transparency = new Transparency(120);

            //Point2dCollection pts = new Point2dCollection();
            //pts.Add(new Point2d(Center.X - Size * 0.45, Center.Y - Size * 0.45));
            //pts.Add(new Point2d(Center.X - Size * 0.45, Center.Y + Size * 0.45));
            //pts.Add(new Point2d(Center.X + Size * 0.45, Center.Y + Size * 0.45));
            //pts.Add(new Point2d(Center.X + Size * 0.45, Center.Y - Size * 0.45));
            //pts.Add(new Point2d(Center.X - Size * 0.45, Center.Y - Size * 0.45));
            //DoubleCollection dps = new DoubleCollection(new double[] { 0, 0, 0, 0,0 });

            //h.AppendLoop(HatchLoopTypes.Default, pts, dps);
            //h.EvaluateHatch(false);
            //return h;
        }
    }
}
