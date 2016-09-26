﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace PIK_GP_Acad.Insolation
{
    /// <summary>
    /// Зона освещенности - контур освещенности для заданной точки
    /// 
    /// </summary>
    public abstract class IlluminationArea : IIlluminationArea
    {
        private IInsolationService insService;
        public Polyline Low { get; set; }
        public Polyline Medium { get; set; }
        public Polyline Hight { get; set; }
        /// <summary>
        /// Угол начала освещенности/тени
        /// </summary>
        public double AngleStartOnPlane { get; set; }
        /// <summary>
        /// Угол конца освещенности/тени
        /// </summary>
        public double AngleEndOnPlane { get; set; }
                
        Point2d pt;

        public IlluminationArea (IInsolationService insService, double angleStart, double angleEnd, Point2d pt)
        {
            this.insService = insService;
            AngleStartOnPlane = angleStart;
            AngleEndOnPlane = angleEnd;
            this.pt = pt;                      
        }

        /// <summary>
        /// Построение контура освещенности
        /// </summary>        
        public void Create (BlockTableRecord space)
        {
            Point2d pt1 = Point2d.Origin;
            Point2d pt2 = Point2d.Origin;
            //Low = CreatePl(insService.Options.VisualOptions[0].Height,
            //            Color.FromColor(insService.Options.VisualOptions[0].Color), true, ref pt1, ref pt2);
            //Medium = CreatePl(insService.Options.VisualOptions[1].Height,
            //    Color.FromColor(insService.Options.VisualOptions[1].Color), false, ref pt1, ref pt2);
            //Hight = CreatePl(insService.Options.VisualOptions[2].Height,
            //    Color.FromColor(insService.Options.VisualOptions[2].Color), false, ref pt1, ref pt2);

            Transaction t = space.Database.TransactionManager.TopTransaction;
            visualPl(Low, space, t);
            visualPl(Medium, space, t);
            visualPl(Hight, space, t);
        }

        private Polyline CreatePl (int height, Color color, bool fromStartPt,ref Point2d pt1,ref Point2d pt2)
        {
            double cShadow;
            var yShadowLen = insService.CalcValues.YShadowLineByHeight(height, out cShadow);
            var yShadow = pt.Y - yShadowLen;
            var xRayToStart = insService.CalcValues.GetXRay(yShadowLen,AngleStartOnPlane);
            var xRayToEnd = insService.CalcValues.GetXRay(yShadowLen, AngleEndOnPlane);
            Polyline pl;
            int index = 1;
            if (fromStartPt)
            {
                pl = new Polyline(3);
                pl.AddVertexAt(0, pt, 0, 0, 0);
            }
            else
            {
                pl = new Polyline(4);
                pl.AddVertexAt(0, pt1, 0, 0, 0);
                pl.AddVertexAt(1, pt2, 0, 0, 0);
                index = 2;
            }
            pt2 = new Point2d(pt.X + xRayToStart, yShadow);
            pl.AddVertexAt(index++, pt2, 0, 0, 0);
            pt1 = new Point2d(pt.X + xRayToEnd, yShadow);
            pl.AddVertexAt(index++, pt1, 0, 0, 0);
            pl.Closed = true;
            pl.Color = color;
            return pl;
        }

        private void visualPl(Polyline pl, BlockTableRecord cs, Transaction t)
        {   
            pl.Transparency = new Transparency(insService.Options.Transparence);
            cs.AppendEntity(pl);
            t.AddNewlyCreatedDBObject(pl, true);
            var h = AcadLib.Hatches.HatchExt.CreateAssociativeHatch(pl, cs, t);
            if (h != null)
            {
                h.Color = pl.Color;
                h.Transparency = new Transparency(insService.Options.Transparence);
            }
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
                }
                else
                {
                    merged.Add(ilum);
                    cur = ilum;
                }
            }
            return merged;
        }
    }
}
