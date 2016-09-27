using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using PIK_GP_Acad.Elements.Buildings;
using PIK_GP_Acad.Model.Insolation.Models;

namespace PIK_GP_Acad.Model.Insolation.ShadowMap
{
    /// <summary>
    /// Ячейка карты - квадрат с координатой центра 
    /// </summary>
    public class Tile
    {
        private static Color ColorD = Color.FromColor(System.Drawing.Color.Blue);
        private static Color ColorC = Color.FromRgb(255, 192,0);
        private static Color ColorB = Color.FromColor(System.Drawing.Color.Yellow);
        private static Color ColorA = Color.FromColor(System.Drawing.Color.Red);
        /// <summary>
        /// Размер ячейки - одинаковый для всех
        /// </summary>
        public static int Size { get; set; }
        /// <summary>
        /// Время тени ячейки от соответствующих зданий
        /// Кол-во массива равно количеству шагов на которые разбиты градусы от первого расячетного градуса до последнего
        /// Время каждого элемента = величине одного шага в минутах (время тени).
        /// </summary>
        public List<IBuilding>[] InsTime { get; set; }
        /// <summary>
        /// Точка центра ячейки
        /// </summary>
        Point3d Center { get; set; }
        InsRate InsRate { get; set; }       

        public Tile(Point3d center)
        {
            Center = center;            
        }

        public Drawable CreateVisual ()
        {
            Autodesk.AutoCAD.DatabaseServices.Polyline pl = new Autodesk.AutoCAD.DatabaseServices.Polyline();
            pl.AddVertexAt(0, new Point2d(Center.X - Size * 0.45, Center.Y - Size * 0.45), 0, 0, 0);
            pl.AddVertexAt(1, new Point2d(Center.X - Size * 0.45, Center.Y + Size * 0.45), 0, 0, 0);
            pl.AddVertexAt(2, new Point2d(Center.X + Size * 0.45, Center.Y + Size * 0.45), 0, 0, 0);
            pl.AddVertexAt(3, new Point2d(Center.X + Size * 0.45, Center.Y - Size * 0.45), 0, 0, 0);
            pl.Closed = true;
            pl.Color = GetInsColor(InsRate);
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

        private static Color GetInsColor (InsRate insRate)
        {
            switch (insRate)
            {
                case InsRate.D:
                    return ColorD;
                case InsRate.C:
                    return ColorC;
                case InsRate.B:
                    return ColorB;
                case InsRate.A:
                    return ColorA;
            }
            // invalid operation
            return Color.FromColorIndex(ColorMethod.ByAci, 9);
        }
    }
}
