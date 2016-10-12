using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using PIK_GP_Acad.Insolation.Models;
using AcadLib;

namespace PIK_GP_Acad.Insolation.Services
{
    public abstract class IllumAreaBase : IIlluminationArea
    {        
        public Point2d PtOrig { get; set; }
        public Point2d PtStart { get; set; }
        public Point2d PtEnd{ get; set; }
        public double AngleEndOnPlane { get; set; }
        public double AngleStartOnPlane { get; set; }
        public int Time { get; set; }        
        public InsPoint InsPoint { get; set; }      

        public IllumAreaBase(Point2d ptOrig,double angleStart, double angleEnd, Point2d ptStart, Point2d ptEnd)
        {
            AngleStartOnPlane = angleStart;
            AngleEndOnPlane = angleEnd;
            PtOrig = ptOrig;
            PtStart = ptStart;
            PtEnd = ptEnd;
        }

        /// <summary>
        /// Объекдинение накладывающихся зон освещенности/тени
        /// </summary>                
        public static List<IIlluminationArea> Merge (List<IIlluminationArea> illums)
        {
            if (illums.Count == 0)
                return illums;

            List<IIlluminationArea> merged = new List<IIlluminationArea>();
            var sortedByStart = illums.OrderBy(o => o.AngleStartOnPlane).ToList();

            IIlluminationArea cur = sortedByStart[0];
            merged.Add(cur);
            foreach (var ilum in sortedByStart.Skip(1))
            {
                if (ilum.AngleStartOnPlane <= cur.AngleEndOnPlane)
                {
                    cur.AngleEndOnPlane = ilum.AngleEndOnPlane;
                    cur.PtEnd = ilum.PtEnd;
                }
                else
                {
                    merged.Add(ilum);
                    cur = ilum;
                }
            }
            return merged;
        }

        public List<Drawable> CreateVisual ()
        {
            List<Drawable> draws = new List<Drawable>();

            // Штриховка
            var visOpt = new VisualOption(System.Drawing.Color.Red, Point3d.Origin, 60);
            var points = new List<Point2d> { PtOrig, PtStart, PtEnd };
            var h = VisualHelper.CreateHatch(points, visOpt);
            draws.Add(h);

            // Угловой размер
            var ptCenter = GetCenterTriangle(PtOrig, PtStart, PtEnd);
            var ptDim1 = PtOrig + (PtStart - PtOrig) / 2;
            var ptDim2 = PtOrig + (PtEnd - PtOrig) / 2;
            var dim = new LineAngularDimension2(PtOrig.Convert3d(), ptDim1.Convert3d(), 
                PtOrig.Convert3d(), ptDim2.Convert3d(), ptCenter.Convert3d(), Time.ToHours() + "ч.", ObjectId.Null);
            dim.Color = Color.FromColor(System.Drawing.Color.Red);
            dim.Dimtxt = 1.5;
            dim.Dimscale = 0.5;

            draws.Add(dim);

            return draws;
        }
        private Point2d GetCenterTriangle (Point2d p1, Point2d p2, Point2d p3)
        {
            var c12 = p1 + (p2 - p1) / 2;
            var c13 = p1 + (p3 - p1) / 2;
            var c = c12 + (c13 - c12) / 2;
            return c;
        }

        /// <summary>
        /// Найти точку на луче (заданном углом) как перпендикуляр от заданной точки
        /// </summary>
        /// <param name="ptOtherRay">Точка на другом луче</param>
        /// <param name="angleRay">Угол луча на котором нужно найти точку. Угол от 0 (восхода) по часовой стрелке</param>
        /// <returns>Определенная точка</returns>
        public static Point2d GetPointInRayFromPoint (Point2d ptOrig, Point2d ptOtherRay, double angleRay)
        {
            Vector2d vecRay = (Vector2d.XAxis * (ptOtherRay - ptOrig).Length).RotateBy(-angleRay);            
            Line2d lineRay = new Line2d(ptOrig, vecRay);
            var closestPt = lineRay.GetClosestPointTo(ptOtherRay);
            var resPt = closestPt.Point;
            return resPt;            
        }

        public static Point2d GetPointInRayByLength (Point2d ptOrig, double angleRayIns, int length)
        {            
            var vecRay = Vector2d.XAxis.RotateBy(-angleRayIns)* length;
            return ptOrig + vecRay;
        }
    }
}
