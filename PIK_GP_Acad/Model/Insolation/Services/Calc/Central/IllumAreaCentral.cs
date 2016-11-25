using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.Insolation.Services;
using PIK_GP_Acad.Insolation.Models;

namespace PIK_GP_Acad.Insolation.Services
{
    /// <summary>
    /// Зона освещенности - контур освещенности для заданной точки
    /// 
    /// </summary>
    public class IllumAreaCentral : IllumAreaBase
    {
        //public Polyline Low { get; set; }
        //public Polyline Medium { get; set; }
        //public Polyline Hight { get; set; }       

        public IllumAreaCentral (IInsPoint insPoint, Point2d ptOrig, double angleStart, double angleEnd, Point2d ptStart, Point2d ptEnd)
            : base(insPoint, ptOrig, angleStart, angleEnd, ptStart, ptEnd)
        {

        }

        ///// <summary>
        ///// Построение контура освещенности
        ///// </summary>        
        //public override void Create (BlockTableRecord space)
        //{
        //    Point2d pt1 = Point2d.Origin;
        //    Point2d pt2 = Point2d.Origin;
        //    //Low = CreatePl(insService.Options.VisualOptions[0].Height,
        //    //            Color.FromColor(insService.Options.VisualOptions[0].Color), true, ref pt1, ref pt2);
        //    //Medium = CreatePl(insService.Options.VisualOptions[1].Height,
        //    //    Color.FromColor(insService.Options.VisualOptions[1].Color), false, ref pt1, ref pt2);
        //    //Hight = CreatePl(insService.Options.VisualOptions[2].Height,
        //    //    Color.FromColor(insService.Options.VisualOptions[2].Color), false, ref pt1, ref pt2);

        //    Transaction t = space.Database.TransactionManager.TopTransaction;
        //    visualPl(Low, space, t);
        //    visualPl(Medium, space, t);
        //    visualPl(Hight, space, t);
        //}

        public static List<IIlluminationArea> Invert (List<IIlluminationArea> illums, 
            IIlluminationArea startAnglesIllumBound, Point2d ptOrig, IInsPoint insPoint)
        {
            double angleStart = startAnglesIllumBound.AngleStartOnPlane;
            double angleEnd= startAnglesIllumBound.AngleEndOnPlane;
            List<IIlluminationArea> inverts = new List<IIlluminationArea>();

            if (illums.Count == 0)
            {
                // Зон теней нет. От стартового угла до конечного - зона освещена                    
                var illum = new IllumAreaCentral(insPoint, ptOrig, angleStart, angleEnd,
                    GetPointInRayByLength(ptOrig, angleStart, 50),
                    GetPointInRayByLength(ptOrig, angleEnd, 50));
                inverts.Add(illum);
            }
            else
            {
                double curStart = angleStart;
                Point2d cusStartPt = GetPointInRayFromPoint(illums[0].PtOrig, illums[0].PtStart, curStart);

                foreach (var item in illums)
                {
                    if (item.AngleStartOnPlane - curStart > 0.01)
                    {
                        var illum = new IllumAreaCentral(insPoint, item.PtOrig, curStart, item.AngleStartOnPlane, cusStartPt, item.PtStart);
                        inverts.Add(illum);
                    }
                    curStart = item.AngleEndOnPlane;
                    cusStartPt = item.PtEnd;
                }
                if (angleEnd - curStart > 0.1)
                {
                    Point2d ptEnd = GetPointInRayFromPoint(illums[0].PtOrig, cusStartPt, angleEnd);
                    var illum = new IllumAreaCentral(insPoint,illums[0].PtOrig, curStart, angleEnd, cusStartPt, ptEnd);
                    inverts.Add(illum);
                }
            }
            return inverts;
        }
        
        //private Polyline CreatePl (int height, Color color, bool fromStartPt,ref Point2d pt1,ref Point2d pt2)
        //{
        //    double cShadow;
        //    var yShadowLen = insService.CalcValues.YShadowLineByHeight(height, out cShadow);
        //    var yShadow = pt.Y - yShadowLen;
        //    var xRayToStart = insService.CalcValues.GetXRay(yShadowLen,AngleStartOnPlane);
        //    var xRayToEnd = insService.CalcValues.GetXRay(yShadowLen, AngleEndOnPlane);
        //    Polyline pl;
        //    int index = 1;
        //    if (fromStartPt)
        //    {
        //        pl = new Polyline(3);
        //        pl.AddVertexAt(0, pt, 0, 0, 0);
        //    }
        //    else
        //    {
        //        pl = new Polyline(4);
        //        pl.AddVertexAt(0, pt1, 0, 0, 0);
        //        pl.AddVertexAt(1, pt2, 0, 0, 0);
        //        index = 2;
        //    }
        //    pt2 = new Point2d(pt.X + xRayToStart, yShadow);
        //    pl.AddVertexAt(index++, pt2, 0, 0, 0);
        //    pt1 = new Point2d(pt.X + xRayToEnd, yShadow);
        //    pl.AddVertexAt(index++, pt1, 0, 0, 0);
        //    pl.Closed = true;
        //    pl.Color = color;
        //    return pl;
        //}

        //private void visualPl(Polyline pl, BlockTableRecord cs, Transaction t)
        //{   
        //    pl.Transparency = new Transparency(insService.Options.Transparence);
        //    cs.AppendEntity(pl);
        //    t.AddNewlyCreatedDBObject(pl, true);
        //    var h = AcadLib.Hatches.HatchExt.CreateAssociativeHatch(pl, cs, t);
        //    if (h != null)
        //    {
        //        h.Color = pl.Color;
        //        h.Transparency = new Transparency(insService.Options.Transparence);
        //    }
        //}        
    }
}
