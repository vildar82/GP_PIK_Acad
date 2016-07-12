using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;

namespace PIK_GP_Acad.Insolation.SunlightRule
{
    public interface ISunlightRule
    {
        /// <summary>
        /// Начальный угол линейки
        /// </summary>
        double StartAngle { get; set; }
        /// <summary>
        /// Конечный угол линейки
        /// </summary>
        double EndAngle { get; set; }

        /// <summary>
        /// Определение длины для высоты
        /// </summary>
        /// <param name="height">Высота, м.</param>
        /// <returns>Радиус, м.</returns>
        int GetLength (int height);
        /// <summary>
        /// Определение высоты точки
        /// </summary>        
        double GetHeightAtPoint (Point3d point, Point3d origin);
        /// <summary>
        /// Определение точки на заданной высоте в указанном направлении
        /// </summary>
        /// <param name="maxHeight"></param>
        /// <param name="pt"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        Point3d GetPointByHeight (int maxHeight, Point3d pt, Point3d origin);
    }
}
