// Условия эксопрта:
// 1. Из расчета что, экспортируется в excel (соответственно, только ортогонально)
// 2. Серия ПИК1, шаг модуля 3.6м.

// На выходе нужно получить - массив ячеек инсоляции. 1 ячейка = 3.6х3.6.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.Insolation.Models;
using AcadLib;
using AcadLib.Geometry;

namespace PIK_GP_Acad.Insolation.Services.Export
{
    /// <summary>
    /// Экпорт инсоляции дома
    /// </summary>
    public class ExportHouseIns
    {
        private House house;
        public ExportHouseIns(House house)
        {
            this.house = house;
        }

        /// <summary>
        /// Получение ячеек инсоляции из дома (фронтов)
        /// </summary>
        /// <returns></returns>
        private List<InsCell> GetCells ()
        {
            var contour = house.Contour;
            var insCells = new List<InsCell>();
            for (int i = 0; i < house.Contour.NumberOfVertices; i++)
            {
                InsCell lastCell = null;
                using (var seg = contour.GetLineSegment2dAt(i))
                {
                    if (seg.Length < 3.6)
                    {
                        continue;
                    }
                    // расчетнве точки этого сегмента
                    var segCalcPoints = house.ContourSegmentsCalcPoints[i];
                    // вектор одного модуля (шага 3,6)
                    var vecModule = seg.Direction * 3.6;

                    // стартовая и конечная точка модуля на сегменте
                    var startModulePt = seg.StartPoint;
                    var endModulePt = startModulePt + vecModule;
                    // текущая расчетная точка (первая для этого модуля)
                    var lastCalcPt = segCalcPoints[0];
                    // Точка центра модуля (квадрата модуля)
                    var centerModulePt = GetCenterModulePoint(contour, startModulePt, vecModule);
                }
            }
            return null;
        }

        /// <summary>
        /// Точка центра модуля
        /// </summary>        
        private Point2d GetCenterModulePoint (Polyline contour, Point2d startModulePt, Vector2d vecModule)
        {
            var vecModuleHalf = vecModule * 0.5;
            var ptCenterVec = startModulePt + vecModuleHalf;
            // Перпендикулярно внутрь полилинии
            var vecModuleHalfPerp = vecModuleHalf.GetPerpendicularVector();
            var ptCenterModule = ptCenterVec + vecModuleHalfPerp;
            if (contour.IsPointInsidePolygon(new Point3d (ptCenterModule.X, ptCenterModule.Y, contour.Elevation)))
            {
                return ptCenterModule;
            }
            else
            {
                return ptCenterVec + vecModuleHalfPerp.Negate();
            }
        }
    }
}
