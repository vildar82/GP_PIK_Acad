using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.Elements.Buildings;
using AcadLib.Geometry;
using AcadLib.Exceptions;
using PIK_GP_Acad.Insolation.Models;

namespace PIK_GP_Acad.Insolation.Services
{
    public class CalcPointCentral
    {
        Plane plane = new Plane();
        InsPoint insPt;
        InsBuilding buildingOwner;
        Map map;
        Point3d ptCalc;
        Point2d ptCalc2d;
        InsCalcServiceCentral calc;
        ICalcValues values;
        /// <summary>
        /// Начальный угол в плане (радиан). Начальное значение = 0 - восход.
        /// Будут определены для этой расвчетной точки индивидуально
        /// </summary>
        public double AngleStartOnPlane { get; private set; }
        /// <summary>
        /// Конечный угол в плане (радиан). Начальное значение = 180 - заход
        /// </summary>
        public double AngleEndOnPlane { get; private set; }        

        public CalcPointCentral (InsPoint insPt, InsCalcServiceCentral insCalcService)
        {
            this.map = insPt.Model.Map;
            buildingOwner = insPt.Building;            
            this.insPt = insPt;
            ptCalc = insPt.Point;
            ptCalc2d = ptCalc.Convert2d();
            this.calc = insCalcService;
            values = insCalcService.CalcValues;

            AngleStartOnPlane = values.SunCalcAngleStartOnPlane;
            AngleEndOnPlane = values.SunCalcAngleEndOnPlane;
        }

        public List<IIlluminationArea> Calc ()
        {
            var resAreas = new List<IIlluminationArea>();
            // Определение ограничений углов (начального и конечного) с учетом плоскости стены расчетного дома
            if (DefineStartAnglesByOwnerBuilding())
            {
                // расчетные граници (по заданным расчетным углам)
                var ext = GetCalcExtents(map.MaxBuildingHeight);
                // кусок карты
                using (var scope = map.GetScope(ext))
                {
                    // исключение из списка домов собственно расчетного дома
                    scope.Buildings.Remove(buildingOwner);

                    // Расчет зон теней
                    // группировка домов по высоте
                    var heights = scope.Buildings.GroupBy(g => g.Height);
                    foreach (var bHeight in heights)
                    {
                        // зоны тени для домов этой высоты
                        var illumsByHeight = CalcIllumsByHeight(bHeight.ToList(), bHeight.Key - insPt.Height);
                        resAreas.AddRange(illumsByHeight);
                    }
                }
                resAreas = IllumAreaBase.Merge(resAreas);

                // Инвертировать зоны теней в зоны освещенностей    
                resAreas = IllumAreaCentral.Invert(resAreas, AngleStartOnPlane, AngleEndOnPlane, ptCalc2d);
            }
            return resAreas;
        }

        private List<IIlluminationArea> CalcIllumsByHeight (List<InsBuilding> buildings, int height)
        {
            List<IIlluminationArea> illumShadows = new List<IIlluminationArea>();

            // катет тени и гипотенуза тени (относительно расчетной точки) - высота линии тени
            double cShadow;
            double yShadowLen = values.YShadowLineByHeight(height, out cShadow);
            double yShadow = ptCalc2d.Y - yShadowLen;

            // Линия тени
            var xRayToStart = values.GetXRay(yShadowLen, AngleStartOnPlane);
            var xRayToEnd = values.GetXRay(yShadowLen, AngleEndOnPlane);
            using (Line lineShadow = new Line(new Point3d(ptCalc.X + xRayToStart, yShadow, 0),
                              new Point3d(ptCalc.X + xRayToEnd, yShadow, 0)))
            {                
                // перебор домов одной высоты
                foreach (var build in buildings)
                {
                    // Если дом полностью выше линии тени (сечения), то он полностью затеняет точку
                    if (build.YMin > yShadow)
                    {
                        // Найти точку начала тени и конца (с минимальным и макс углом к точке расчета)
                        var ilumShadow = GetIllumShadow(build.Contour.GetPoints());
                        if (ilumShadow != null)
                        {
                            illumShadows.Add(ilumShadow);
                        }
                    }
                    else if (build.YMax > yShadow)
                    {
                        var ilumsBoundary = GetBuildingLineShadowBoundary(build, lineShadow, Intersect.ExtendThis);
                        illumShadows.AddRange(ilumsBoundary);
                    }
                }
            }
            // Объединение совпадающих зон теней
            illumShadows = IllumAreaBase.Merge(illumShadows);
            return illumShadows;
        }

        private List<IIlluminationArea> GetBuildingLineShadowBoundary (InsBuilding build, Line lineShadow,
            Intersect intersectMode)
        {
            List<IIlluminationArea> resIlumsShadows = new List<IIlluminationArea>();
            if (lineShadow.Length < 1) return resIlumsShadows;
            // Дом на границе тени, нужно строить линию пересечения с домом
            Point3dCollection ptsIntersects = new Point3dCollection();
            lineShadow.IntersectWith(build.Contour, intersectMode, plane, ptsIntersects, IntPtr.Zero, IntPtr.Zero);
            // Точки выше найденного пересецения
            var ptsContour = build.Contour.GetPoints();
            var ptsAboveLine = ptsContour.Where(p => p.Y >= lineShadow.StartPoint.Y).ToList();
            foreach (Point3d item in ptsIntersects)
            {
                ptsAboveLine.Add(item.Convert2d());
            }
            var ilumShadow = GetIllumShadow(ptsAboveLine);
            if (ilumShadow != null)
            {
                resIlumsShadows.Add(ilumShadow);
            }            
            return resIlumsShadows;
        }

        /// <summary>
        /// Определение граничных точек задающих тень по отношению к расчетной точке, среди всех точек
        /// </summary>        
        private IIlluminationArea GetIllumShadow (List<Point2d> points)
        {
            IIlluminationArea ilum = null;
            // список точек и их углов к расчетной точке
            List<Tuple<Point2d, double>> angles = new List<Tuple<Point2d, double>>();
            foreach (var iPt in points)
            {
                // угол к расчетной точке (от 0 по часовой стрелке)
                if (!ptCalc2d.IsEqualTo(iPt))
                {
                    var angle = values.GetInsAngleFromAcad((iPt - ptCalc2d).Angle);
                    angles.Add(new Tuple<Point2d, double>(iPt, angle));
                }
            }
            if (angles.Count > 1)
            {
                angles.Sort((p1, p2) => p1.Item2.CompareTo(p2.Item2));

                ilum = CreateIllumShadow(angles[0], angles[angles.Count - 1]);
            }
            return ilum;
        }

        private IIlluminationArea CreateIllumShadow (Tuple<Point2d, double> angleStart, Tuple<Point2d, double> angleEnd)
        {
            if (angleEnd.Item2 - angleStart.Item2 > Math.PI)
            {
                // Переворот начального и конечного угла
                var t1 = angleEnd;
                angleEnd = angleStart;
                angleStart = t1;
            }

            // если конечный угол меньше начального расчетного или наоборот, то тень вне границ расчета. Или если начальный угол = конечному
            if (angleEnd.Item2 < AngleStartOnPlane || angleStart.Item2 > AngleEndOnPlane || angleStart.Item2.IsEqual(angleEnd.Item2, 0.001))
                return null;

            if (angleStart.Item2 < AngleStartOnPlane)
            {
                var ptStart = IllumAreaBase.GetPointInRayFromPoint(ptCalc2d, angleStart.Item1, AngleStartOnPlane);
                angleStart = new Tuple<Point2d, double>(ptStart, AngleStartOnPlane);
            }
            if (angleEnd.Item2 > AngleEndOnPlane)
            {
                var ptEnd = IllumAreaBase.GetPointInRayFromPoint(ptCalc2d, angleEnd.Item1, AngleEndOnPlane);
                angleEnd = new Tuple<Point2d, double>(ptEnd, AngleEndOnPlane);
            }
            var ilum = new IllumAreaCentral(ptCalc2d, angleStart.Item2, angleEnd.Item2, angleStart.Item1, angleEnd.Item1);
            return ilum;
        }

        /// <summary>
        /// Расчетная область - от точки
        /// </summary>        
        private Extents3d GetCalcExtents (int maxHeight)
        {
            // высота тени - отступ по земле от расчетной точки до линии движения тени
            double cSunPlane;
            double ySunPlane = values.YShadowLineByHeight(maxHeight, out cSunPlane);
            // растояние до точки пересечения луча и линии тени
            double xRayToStart = values.GetXRay(ySunPlane, AngleStartOnPlane);
            double xRayToEnd = values.GetXRay(ySunPlane, AngleEndOnPlane);
            Extents3d ext = new Extents3d(new Point3d(ptCalc.X + xRayToEnd, ptCalc.Y - ySunPlane, 0),
                                          new Point3d(ptCalc.X + xRayToStart, ptCalc.Y, 0));
            return ext;
        }

        private bool DefineStartAnglesByOwnerBuilding ()
        {
            buildingOwner.InitContour();
            using (var contour = buildingOwner.Contour)
            {
                // Если полилиния дома полностью выше или равна расчетной точке - то точка полность освещен (сам себя не загораживает никак)
                if (buildingOwner.ExtentsInModel.MinPoint.Y >= ptCalc.Y)
                {
                    return true;
                }
                // Если дом полностью нижке расчетной точки - то дом полностью загожен сам собой
                if (buildingOwner.ExtentsInModel.MaxPoint.Y <= ptCalc.Y)
                {
                    return false;
                }

                // Проверка ограничения от самого здания с восточной стороны
                CorrectStartAnglesByOwnerZeroLineIntersects(contour);

                // Ограничения от окна
                CorrectStartAnglesByWindow(contour);
            }

            // Стартовый угол не может быть больше конечного
            if (IsAllShadow()) return false;
            // Конечный угол не может быть больше Pi
            if (AngleEndOnPlane >= Math.PI)
            {
                throw new Exception("Ошибка расчета ограничивающих углов.");
            }
            return true;
        }                

        /// <summary>
        /// Определение стартовых углов от собственной плоскости окна с учетом отступа
        /// </summary>        
        private void CorrectStartAnglesByWindow (Polyline contour)
        {
            var segStartIndex = (int)contour.GetParameterAtPoint(ptCalc);
            var segOwner = contour.GetLineSegment2dAt(segStartIndex);

            // Вектор перпендикуляроно выходящий из окна во вне дома
            var vecWindPerp = GetWindowPerpVector(segOwner, contour);
            // Восточный угол = повороту перп вектороа окна на 90 (по автокадовски)
            var eastVec = vecWindPerp.RotateBy(MathExt.PIHalf);
            var eastAngle = values.GetInsAngleFromAcad(eastVec.Angle);
            // Учет теневого угла окна
            eastAngle += insPt.Window.ShadowAngle;
            if (eastAngle > AngleStartOnPlane && eastAngle < Math.PI)
            {
                AngleStartOnPlane = eastAngle;
            }
            // Западный угол            
            var westAngle = values.GetInsAngleFromAcad(eastVec.Angle + Math.PI);
            westAngle -= insPt.Window.ShadowAngle;
            if (westAngle < AngleEndOnPlane)
            {
                AngleEndOnPlane = westAngle;
            }
        }

        private Vector2d GetWindowPerpVector (LineSegment2d segOwner, Polyline contour)
        {
            var vecSegPerp = segOwner.Direction.GetPerpendicularVector().GetNormal();
            var pt = ptCalc2d + vecSegPerp;
            if (contour.IsPointInsidePolygon(pt.Convert3d()))
            {
                vecSegPerp = vecSegPerp.Negate();
            }
            return vecSegPerp;
        }        

        private bool IsAllShadow ()
        {
            return AngleStartOnPlane >= AngleEndOnPlane;
        }

        private void CorrectStartAnglesByOwnerZeroLineIntersects (Polyline contour)
        {
            // Линия через точку ноль.
            using (var lineZero = new Line(ptCalc, new Point3d(ptCalc.X + 50, ptCalc.Y, 0)))
            {        
                var ptsIntersectCol = new Point3dCollection();
                contour.IntersectWith(lineZero, Intersect.ExtendArgument, new Plane(), ptsIntersectCol, IntPtr.Zero, IntPtr.Zero);
                var ptsIntersectSort = ptsIntersectCol.Cast<Point3d>().OrderBy(o => o.X).ToList();
                // Разделить точки левее стартовой и правее (Запад/Восток) - для каждой петли свойестороны найти максимальный ограничивающий угол.                    
                var westAngle = GetStartAngleBySideIntersects(contour, ptsIntersectSort, false);
                if (westAngle < AngleEndOnPlane)
                {
                    AngleEndOnPlane = westAngle;
                }
                var eastAngle = GetStartAngleBySideIntersects(contour, ptsIntersectSort, true);
                if (eastAngle > AngleStartOnPlane)
                {
                    AngleStartOnPlane = eastAngle;
                }
            }
        }

        /// <summary>
        /// Корректировка стартового угла ограницения от дома - по точкам пересечения полилинии с горизонталью
        /// </summary>
        /// <param name="contour">Контур</param>
        /// <param name="ptsIntersectSortByX">все точки пересечения, отсортированные по X</param>
        /// <param name="isEastSide">Сторона восток - запад</param>
        private double GetStartAngleBySideIntersects (Polyline contour, List<Point3d> ptsIntersectSortByX, bool isEastSide)
        {
            if (ptsIntersectSortByX.Count == 0) return 0;
            double angleRes = 0;

            List<Point3d> ptsIntersectSide;
            bool maxOrMinAngle;
            if (isEastSide)
            {
                ptsIntersectSide = ptsIntersectSortByX.Where(p => p.X >= ptCalc.X).ToList();
                maxOrMinAngle = true;
            }
            else
            {
                ptsIntersectSide = ptsIntersectSortByX.Where(p => p.X <= ptCalc.X).ToList();
                maxOrMinAngle = false;
                angleRes = Math.PI;
            }

            var ptPrew = ptsIntersectSide[0];
            foreach (var pt in ptsIntersectSide.Skip(1))
            {
                // средняя точка должна быть внутри полилинии                
                var ptCentre = ptPrew + (pt- ptPrew)/2;                
                if (contour.IsPointInsidePolygon(ptCentre) && !contour.IsPointOnPolyline(ptCentre))
                {
                    var ptsLoopBelow = contour.GetLoopSideBetweenHorizontalIntersectPoints(ptPrew, pt, false, false);
                    // Определение угла для каждой точки
                    foreach (var ptLoop in ptsLoopBelow)
                    {
                        var angleInsPt = GetInsAngleFromPoint(ptLoop);
                        if ((maxOrMinAngle && (angleInsPt > angleRes)) ||
                            (!maxOrMinAngle && (angleInsPt < angleRes)))
                        {
                            angleRes = angleInsPt;
                        }
                    }
                }
                ptPrew = pt;
            }        
            return angleRes;
        }

        /// <summary>
        /// Угол точки - угол солнца на плочкости
        /// </summary>
        /// <param name="ptLoop">Точка</param>
        /// <returns>Угол инс в радианах</returns>
        private double GetInsAngleFromPoint (Point2d ptLoop)
        {
            var angleAcad = (ptLoop - ptCalc2d).Angle;
            var angleIns = values.GetInsAngleFromAcad(angleAcad);
            return angleIns;
        }
    }
}
