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
                var scope = map.GetScope(ext);
                // исключение из списка домов собственно расчетного дома
                scope.Buildings.Remove(insPt.Building);

                // Расчет зон теней
                // группировка домов по высоте
                var heights = scope.Buildings.GroupBy(g => g.Height);
                foreach (var bHeight in heights)
                {
                    // зоны тени для домов этой высоты
                    var illumsByHeight = CalcIllumsByHeight(bHeight.ToList(), bHeight.Key-insPt.Height);
                    resAreas.AddRange(illumsByHeight);
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
            Line lineShadow = new Line(new Point3d(ptCalc.X + xRayToStart, yShadow, 0),
                              new Point3d(ptCalc.X + xRayToEnd, yShadow, 0));
            //#if DEBUG
            //            var cs = map.Doc.Database.CurrentSpaceId.GetObject(OpenMode.ForWrite) as BlockTableRecord;
            //            var t = cs.Database.TransactionManager.TopTransaction;
            //            cs.AppendEntity(lineShadow);
            //            t.AddNewlyCreatedDBObject(lineShadow, true);
            //#endif
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
            bool isOdd = true;
            Point2d pt1 = Point2d.Origin;
            Point3d[] ptsIntersectsSorted = new Point3d[ptsIntersects.Count];
            ptsIntersects.CopyTo(ptsIntersectsSorted, 0);
            ptsIntersectsSorted = ptsIntersectsSorted.OrderBy(o => o.X).ToArray();
            for (int i = 0; i < ptsIntersectsSorted.Length; i++)
            {
                isOdd = !isOdd;
                var pt = ptsIntersectsSorted[i].Convert2d();
                if (isOdd)
                {
                    // Найти точки полилинии выше точек пересечения
                    var points = GetLoopPointsAbove(build.Contour, pt1, pt);

                    var ilumShadow = GetIllumShadow(points);

                    if (ilumShadow != null)
                    {
                        resIlumsShadows.Add(ilumShadow);
                    }
                }
                pt1 = pt;
            }
            return resIlumsShadows;
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
            bool res = true;
            double windowStartAngle;
            double windowEndAngle;
            Vector2d vecWindowOutPerp;
            Vector2d vecWinToEast;
            Vector2d vecWinToWest;
            // Углы плоскости окна
            DefineWindowSegmentAngles(out windowStartAngle, out windowEndAngle,
                out vecWindowOutPerp, out vecWinToEast, out vecWinToWest);

            // Угол восточной плоскости окна больше стартового угла
            if (windowStartAngle > AngleStartOnPlane)
            {
                if (windowStartAngle< AngleEndOnPlane)
                {
                    AngleStartOnPlane = windowStartAngle;
                }
                else
                {
                    if (windowEndAngle <= AngleStartOnPlane || windowEndAngle >= AngleEndOnPlane)
                    {
                        AngleStartOnPlane = windowStartAngle;
                    }
                }                                
            }   
            if (windowEndAngle < AngleEndOnPlane)
            {
                AngleEndOnPlane = windowEndAngle;
            }

            // Если стартовый угол больше конечного - то освещения вообще нет
            if (AngleStartOnPlane >= AngleEndOnPlane)
            {
                res = false;
            }
            else
            {
                var buildingOwner = insPt.Building;
                // катет тени и гипотенуза тени (относительно расчетной точки) - высота линии тени
                double cShadow;
                double yShadowLen = values.YShadowLineByHeight(buildingOwner.Height-insPt.Height, out cShadow);                

                double windowOutPerpendAngle = values.GetInsAngleFromAcad(vecWindowOutPerp.Angle);

                // Проверка ограничения от самого здания с восточной стороны
                if (windowOutPerpendAngle > AngleStartOnPlane)
                {
                    DefineRestrictionAngleInSide(vecWinToEast, yShadowLen, true);
                }

                if (AngleStartOnPlane >= AngleEndOnPlane)
                {
                    res = false;
                }
                else
                {
                    // Проверка ограничения от самого здания с западной стороны
                    if (windowOutPerpendAngle < AngleEndOnPlane)
                    {
                        DefineRestrictionAngleInSide(vecWinToWest, yShadowLen, false);
                    }
                }

                if (res)
                {
                    // Окончательная проверка углов
                    // Стартовый угол не может быть больше конечного
                    if (AngleStartOnPlane >= AngleEndOnPlane)
                    {
                        res = false;
                    }
                    else
                    {
                        // Конечный угол не может быть больше Pi
                        if (AngleEndOnPlane >= Math.PI)
                        {
                            throw new Exception("Ошибка расчета ограничивающих углов.");
                        }
                    }
                }



                //    // Линия сечения здания проходящая через расчетную точку (горизонтально)
                //    Line lineEast = new Line(new Point3d(ptCalc.X, ptCalc.Y, 0),
                //                        new Point3d(buildingOwner.ExtentsInModel.MaxPoint.X, ptCalc.Y, 0));
                //var ilumsEastBoundary = GetBuildingLineShadowBoundary(buildingOwner, lineEast, false, Intersect.OnBothOperands);
                //if (ilumsEastBoundary.Count > 0)
                //{
                //    var ilum = ilumsEastBoundary[0];
                //    if (ilum.AngleEndOnPlane > angleStartOnPlane)
                //    {
                //        angleStartOnPlane = ilum.AngleEndOnPlane;
                //    }
                //}

                //Line lineWest = new Line(new Point3d(buildingOwner.ExtentsInModel.MinPoint.X, ptCalc.Y, 0),
                //                        new Point3d(ptCalc.X, ptCalc.Y, 0));
                //var ilumsWestBoundary = GetBuildingLineShadowBoundary(buildingOwner, lineWest, false, Intersect.OnBothOperands);
                //if (ilumsWestBoundary.Count > 0)
                //{
                //    var ilum = ilumsWestBoundary[0];
                //    if (ilum.AngleStartOnPlane < angleEndOnPlane)
                //    {
                //        angleEndOnPlane = ilum.AngleStartOnPlane;
                //    }
                //}
            }
            return res;
        }

        /// <summary>
        /// Определение граничных точек задающих тень по отношению к расчетной точке, среди всех точек объекта
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
                    var angle = values.GetInsAngleFromAcad((iPt -ptCalc2d).Angle);                    
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
            if (angleEnd.Item2 -angleStart.Item2 > Math.PI)
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
                var ptStart = IllumAreaBase.GetPointInRayPerpendicularFromPoint(ptCalc2d, angleStart.Item1, AngleStartOnPlane);
                angleStart = new Tuple<Point2d, double>(ptStart, AngleStartOnPlane);
            }
            if (angleEnd.Item2 > AngleEndOnPlane)
            {
                var ptEnd = IllumAreaBase.GetPointInRayPerpendicularFromPoint(ptCalc2d, angleEnd.Item1, AngleEndOnPlane);
                angleEnd = new Tuple<Point2d, double>(ptEnd, AngleEndOnPlane);
            }
            var ilum = new IllumAreaCentral(ptCalc2d, angleStart.Item2, angleEnd.Item2, angleStart.Item1, angleEnd.Item1);
            return ilum;
        }

        private Vector2d GetVecPerpWindToSouth (Point3d pt, Polyline contour, Vector2d vecWinPerpend)
        {
            var linePerp = new Line(pt, pt + new Vector3d(new Plane(), vecWinPerpend));
            var ptsIntersect = new Point3dCollection();
            contour.IntersectWith(linePerp, Intersect.ExtendArgument, new Plane(), ptsIntersect, IntPtr.Zero, IntPtr.Zero);
            var ptInner = ptsIntersect.Cast<Point3d>().Where(p => !p.IsEqualTo(pt)).
                GroupBy(p => (p - pt).Length).OrderBy(o => o.Key).First().First().Convert2d();
            return pt.Convert2d()- ptInner;
        }

        /// <summary>
        /// Определение стартовых углов от собственной плоскости окна с учетом отступа
        /// </summary>
        /// <param name="windowStartAngle">Стартовый угол (Восток)</param>
        /// <param name="windowEndAngle">Конечный угол (Запад)</param>        
        /// <param name="vecWindowOutPerp">Вектор перпендикуляр от окна наружу от здания</param>
        /// <param name="vecWinToEast">Вектор в плоскости окна в сторону восточной вершины сегмента окна</param>
        /// <param name="vecWinToWest">Вектр в плоскости окна в сторону западной вершины сегмена окна</param>
        private void DefineWindowSegmentAngles (out double windowStartAngle, out double windowEndAngle,
            out Vector2d vecWindowOutPerp, out Vector2d vecWinToEast, out Vector2d vecWinToWest)
        {
            var contour = insPt.Building.Contour;
            var segStartIndex = (int)contour.GetParameterAtPoint(ptCalc);
            var segOwner = contour.GetLineSegment2dAt(segStartIndex);
            // линия перпендикулярно точке
            var vecPerpend = segOwner.Direction.GetPerpendicularVector();
            vecWindowOutPerp = GetVecPerpWindToSouth(ptCalc, contour, vecPerpend);
            var angleToEast = values.GetInsAngleFromAcad(vecWindowOutPerp.Angle + MathExt.PIHalf);

            var vecSegStart = segOwner.StartPoint - ptCalc2d;
            var vecSegEnd = segOwner.EndPoint - ptCalc2d;            

            if (Math.Abs(values.GetInsAngleFromAcad(vecSegStart.Angle) - angleToEast) < 0.1)
            {
                // стартовая точка оконного сегмента - направлена на восток                
                vecWinToEast = vecSegStart;
                vecWinToWest = vecSegEnd;
            }
            else
            {
                // стартовая точка оконного сегмента - направлена на запад
                vecWinToEast = vecSegEnd;
                vecWinToWest = vecSegStart;
            }
            windowStartAngle = values.GetInsAngleFromAcad(vecWinToEast.Angle);
            windowEndAngle = values.GetInsAngleFromAcad(vecWinToWest.Angle);

            // Корректировка с учетом отступа (учет конструкции окна)
            windowStartAngle += insPt.Window.ShadowAngle;
            windowEndAngle -= insPt.Window.ShadowAngle;
        }

        /// <summary>
        /// Найти точки петли полилинии выше точек сечения
        /// </summary>
        /// <param name="contour">Исходная Полилиния</param>
        /// <param name="pt1">Первая точка петли (пересечения)</param>
        /// <param name="pt2">Вторая точка петли (пересечения)</param>
        /// <returns>Список точек петли выше пересечения (включая точки пересечения)</returns>
        private List<Point2d> GetLoopPointsAbove (Polyline contour, Point2d pt1, Point2d pt2)
        {
            Tolerance tolerance = new Tolerance(0.001, 0.1);
            List<Point2d> pointsLoopAbove = new List<Point2d>();

            pointsLoopAbove.Add(pt1);
            int numVertex = contour.NumberOfVertices;

            var param = contour.GetParameterAtPoint(pt1.Convert3d());
            int indexMin = (int)param;
            int indexMax = (int)Math.Ceiling(param);
            var seg = contour.GetLineSegmentAt(indexMin);
            int dir = 1;
            int index = indexMax;
            if (seg.StartPoint.Y > seg.EndPoint.Y)
            {
                index = indexMin;
                dir = -1;
            }

            bool isContinue = true;
            int countWhile = 0;
            do
            {
                if (index == -1)
                {
                    index = numVertex - 1;
                }
                else if (index == numVertex)
                {
                    index = 0;
                }
                var pt = contour.GetPoint2dAt(index);
                isContinue =pt.Y > pt2.Y;
                if (isContinue)
                {
                    if (!pt.IsEqualTo(pointsLoopAbove[pointsLoopAbove.Count - 1], tolerance))
                    {
                        pointsLoopAbove.Add(pt);
                    }
                    index += dir;
                }
                countWhile++;
            } while (isContinue && countWhile <= numVertex);
            pointsLoopAbove.Add(pt2);
            return pointsLoopAbove;
        }

        /// <summary>
        /// Вершины полилинии с указанной стороны от заданной точки
        /// </summary>
        /// <param name="contour">Исходная Полилиния</param>
        /// <param name="ptOrig">Первая точка (пересечения)</param>        
        /// <param name="vecPerpendSide">Вектор укзывающий сторону от начальной точки (перпендикуляр к линии стороны)</param>
        /// <returns>Список точек с указанной стороны</returns>
        private List<Point2d> GetVertexInSide (Polyline contour, Point2d ptOrig, Vector2d vecPerpendSide,
            Func<Point2d, bool> additionalCond = null)
        {
            List<Point2d> vertexes = new List<Point2d>();

            int numVertex = contour.NumberOfVertices;

            for (int i = 0; i < numVertex; i++)
            {
                var vertex = contour.GetPoint2dAt(i);
                if (additionalCond == null || additionalCond(vertex))
                {
                    if (Math.Abs((vertex - ptOrig).Angle - vecPerpendSide.Angle) < MathExt.PIHalf)
                    {
                        vertexes.Add(vertex);
                    }
                }
            }
            return vertexes;
        }

        private double GetMaxRestrictionAngle (List<Point2d> vertexesSide, Func<double, bool> conditionAngle)
        {
            var res = vertexesSide.GroupBy(g => values.GetInsAngleFromAcad((g - ptCalc2d).Angle)).
                Where(a => conditionAngle(a.Key)).Select(s => s.Key).DefaultIfEmpty().Max();
            return res;
        }
        private double GetMinRestrictionAngle (List<Point2d> vertexesSide, Func<double, bool> conditionAngle)
        {
            var res = vertexesSide.GroupBy(g => values.GetInsAngleFromAcad((g - ptCalc2d).Angle)).
                Where(a => conditionAngle(a.Key)).Select(s => s.Key).DefaultIfEmpty().Min();
            return res;
        }

        private void DefineRestrictionAngleInSide (Vector2d vecWinToSide, double yShadow, bool isEast)
        {
            // Точки со стороны восточной стороны от окна (только в заданной теневой высоте)
            var vertexesSide = GetVertexInSide(insPt.Building.Contour, ptCalc2d, vecWinToSide,
                (p) =>
                {
                    var yV = ptCalc2d.Y - p.Y;
                    return yV > 0 && yV <= yShadow;
                });

            // Найти ограничивающий угол - минимальный в пределах Pi (по инс измерению)
            double angleLimitByOwnerBuilding;
            if (isEast)
            {
                angleLimitByOwnerBuilding = GetMaxRestrictionAngle(vertexesSide,
                d => d < AngleEndOnPlane && d > AngleStartOnPlane);
            }
            else
            {
                angleLimitByOwnerBuilding = GetMinRestrictionAngle(vertexesSide,
                d => d> AngleStartOnPlane && d < AngleEndOnPlane);
            }            

            if (angleLimitByOwnerBuilding != 0)
                AngleEndOnPlane = angleLimitByOwnerBuilding;
        }
    }
}
