using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace PIK_GP_Acad.Insolation.SunlightRule
{
    /// <summary>
    /// Простая инсоляционная линейка
    /// </summary>
    public class SimpleRule : ISunlightRule
    {
        private double ratioLength = 1.428147987;

        public double EndAngle { get; set; } = 168;

        public double StartAngle { get; set; } = 12;        

        /// <summary>
        /// Определение длины по высоте
        /// </summary>        
        public int GetLength (int height)
        {
            var len = Convert.ToInt32(height * ratioLength);
            return len;
        }

        /// <summary>
        /// Определение высоты точки
        /// </summary>
        /// <param name="point">Точка определения высоты</param>
        /// <param name="origin">Начало</param>
        /// <returns>Высота, м</returns>
        public double GetHeightAtPoint (Point3d point, Point3d origin)
        {
            // Определение длины проекции на ось Y
            var vec = point.Convert2d() - origin.Convert2d();            
            var height = Math.Abs(vec.Y / ratioLength);
            return height;
        }

        public Point3d GetPointByHeightInVector (Point3d ptOrigin, Vector2d vec, int height)
        {
            var b = GetLength(height); 
            var c = b / Math.Abs(Math.Sin(vec.Angle));                        
            var ptRes = ptOrigin.Convert2d() + vec.GetNormal() * c;
            return ptRes.Convert3d();
        }

        public Extents3d GetScanExtents (Point3d pt, int height)
        {
            var lenH = GetLength(height);
            var a = lenH * Math.Tan((90- StartAngle).ToRadians());
            var minPt = new Point3d(pt.X-a,pt.Y -lenH, 0);
            var maxPt = new Point3d(pt.X+ a,pt.Y, 0);
            var ext = new Extents3d(minPt, maxPt);
            return ext;
        }
    }
}
