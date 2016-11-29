using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.Insolation.Models;
using AcadLib;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.Insolation.Services
{
    public class FrontCalcPoint
    {
        public FrontCalcPoint (Point2d pt, bool isCorner)
        {
            Point = pt;
            IsCorner = isCorner;
        }

        public bool IsCorner { get; set; }
        public Point2d Point { get; set; }
        public MapBuilding Section { get; set; }
        /// <summary>
        /// Игнорировать точку - не расчитывать
        /// </summary>
        public bool IsIgnoredPoint { get; set; }
        public InsRequirementEnum InsValue { get; set; }
        public bool IsCalulated { get; set; }

        public bool DefineSection (Map map)
        {
            bool res = false;
            if (IsIgnoredPoint) return res;

            if (Section == null)
            {
                Section = map.GetBuildingInPoint(Point);
                if (Section == null)
                {
                    IsIgnoredPoint = true;
                    res = false;
                }
                else
                {
                    res = true;
#if TEST
                    //EntityHelper.AddEntityToCurrentSpace((Polyline)Section.Contour.Clone());
#endif
                }
            }
            else
            {
                res = true;
            }
            return res;
        }
    }
}
