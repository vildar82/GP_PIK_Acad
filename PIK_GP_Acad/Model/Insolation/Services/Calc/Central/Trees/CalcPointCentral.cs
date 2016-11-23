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
using Autodesk.AutoCAD.Colors;

namespace PIK_GP_Acad.Insolation.Services
{
    public class CalcPointCentral
    {
        Plane plane = new Plane();
        InsPoint insPt;
        MapBuilding buildingOwner;
        Map map;
        Point3d ptCalc;
        Point2d ptCalc2d;
        CalcServiceCentral calc;
        ICalcValues values;
        public IllumAreaBase StartAnglesIllum { get; set; }
        public bool WithOwnerBuilding { get; set; }

        ///// <summary>
        ///// Начальный угол в плане (радиан). Начальное значение = 0 - восход.
        ///// Будут определены для этой расвчетной точки индивидуально
        ///// </summary>
        //public double AngleStartOnPlane { get; private set; }
        ///// <summary>
        ///// Конечный угол в плане (радиан). Начальное значение = 180 - заход
        ///// </summary>
        //public double AngleEndOnPlane { get; private set; }

        public CalcPointCentral (InsPoint insPt, CalcServiceCentral insCalcService)
        {
            this.map = insPt.Model.Map;
            buildingOwner = insPt.Building;            
            this.insPt = insPt;
            ptCalc = insPt.Point;
            ptCalc2d = ptCalc.Convert2d();
            this.calc = insCalcService;
            values = insCalcService.CalcValues;

            //AngleStartOnPlane = values.SunCalcAngleStartOnPlane;
            //AngleEndOnPlane = values.SunCalcAngleEndOnPlane;
            StartAnglesIllum = new IllumAreaBase(ptCalc2d, values.SunCalcAngleStartOnPlane, values.SunCalcAngleEndOnPlane,
                Point2d.Origin, Point2d.Origin);
        }

        public List<IIlluminationArea> Calc ()
        {
            var resAreas = new List<IIlluminationArea>();

            // Корректировка расчетной точки
            if (!CorrectCalcPoint()) return null;

            // Проверка - если точка расположена внутри другого дома (кроме собственного), то вся точка в тени
            if (IsCalcPointInsideOtherBuilding())
            {
                return null;
            }

            // Определение ограничений углов (начального и конечного) с учетом плоскости стены расчетного дома
            if (DefineStartAnglesByOwnerBuilding())
            {
                // расчетные граници (по заданным расчетным углам)
                var ext = GetCalcExtents(map.MaxBuildingHeight);
                // кусок карты
                using (var scope = map.GetScope(ext))
                {
                    // исключение из списка домов собственно расчетного дома
                    if (buildingOwner!= null)
                        scope.Buildings.Remove(buildingOwner);

                    // Расчет зон теней
                    // группировка домов по высоте
                    var heights = scope.Buildings.GroupBy(g => g.HeightCalc);
                    // высота точки с учетом уровня дома
                    var ptHeightCalc = insPt.Height + (buildingOwner?.Building.Elevation ?? 0);
                    foreach (var bHeight in heights)
                    {
                        // зоны тени для домов этой высоты
                        // Расчетная высота точки
                        var heightCalc = bHeight.Key - ptHeightCalc;
                        var illumsByHeight = CalcIllumsByHeight(bHeight.ToList(), heightCalc);
                        if (illumsByHeight != null && illumsByHeight.Any())
                        {
                            resAreas.AddRange(illumsByHeight);
                        }
                    }
                }
                resAreas = IllumAreaBase.Merge(resAreas);

                // Инвертировать зоны теней в зоны освещенностей    
                resAreas = IllumAreaCentral.Invert(resAreas, StartAnglesIllum, ptCalc2d);                
            }
            else
            {
                StartAnglesIllum.AngleEndOnPlane = 0;
                StartAnglesIllum.AngleStartOnPlane = 0;
            }            
            return resAreas;
        }

        /// <summary>
        /// Если расчетная точка находится внутри другого дома.
        /// </summary>        
        private bool IsCalcPointInsideOtherBuilding ()
        {
            var buildings = map.GetBuildingsInPoint(ptCalc2d);
            if (buildings != null && buildings.Any())
            {
                if (buildingOwner != null)
                {
                    buildings.Remove(buildingOwner);
                }
                if (buildings.Any())
                {
                    return true;
                }
            }
            return false;
        }

        private bool CorrectCalcPoint()
        {
            if (!WithOwnerBuilding || buildingOwner == null) return true;
            //buildingOwner.InitContour();
            //using (var contour = buildingOwner.Contour)
            //{
            var correctPt = buildingOwner.Contour.GetClosestPointTo(ptCalc, false);
            if ((correctPt - ptCalc).Length > 2)
            {
#if TEST
                //var dbPt = new DBPoint(correctPt);
                //dbPt.Color = Color.FromColor(System.Drawing.Color.AliceBlue);
                //EntityHelper.AddEntityToCurrentSpace(dbPt);
                //dbPt = new DBPoint(ptCalc);
                //dbPt.Color = Color.FromColor(System.Drawing.Color.Gold);
                //EntityHelper.AddEntityToCurrentSpace(dbPt);
                //EntityHelper.AddEntityToCurrentSpace((Polyline)buildingOwner.Contour.Clone());
                //throw new Exception();
#endif
                return false;
                //throw new Exception("Не корректное положение расчетной точки - " + ptCalc);
            }
            ptCalc = correctPt;
            ptCalc2d = correctPt.Convert2d();
            StartAnglesIllum.PtOrig = ptCalc2d;
            //}                                  
            return true;
        }

        private List<IIlluminationArea> CalcIllumsByHeight (List<MapBuilding> buildings, double height)
        {
            List<IIlluminationArea> illumShadows = new List<IIlluminationArea>();           
            
            using (Line lineShadow = GetLineShadow(height))
            {                
                // перебор домов одной высоты
                foreach (var build in buildings)
                {
                    // Если дом полностью выше линии тени (сечения), то он полностью затеняет точку
                    if (build.YMin >= lineShadow.StartPoint.Y)
                    {
                        // Найти точку начала тени и конца (с минимальным и макс углом к точке расчета)
                        var ilumShadow = GetIllumShadow(build.Contour.GetPoints().Where(p=>p.Y<ptCalc.Y).ToList());
                        if (ilumShadow != null)
                        {
                            illumShadows.Add(ilumShadow);
                        }
                    }
                    else if (build.YMax >= lineShadow.StartPoint.Y)
                    {
                        var ilumsBoundary = GetBuildingLineShadowBoundary(build, lineShadow, Intersect.ExtendThis);
                        illumShadows.AddRange(ilumsBoundary);
                    }
                }
#if TEST                
                EntityHelper.AddEntityToCurrentSpace(lineShadow);
#endif
            }
            // Объединение совпадающих зон теней
            illumShadows = IllumAreaBase.Merge(illumShadows);
            return illumShadows;
        }        

        private List<IIlluminationArea> GetBuildingLineShadowBoundary (MapBuilding build, Line lineShadow,
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
            if (angleEnd.Item2 < StartAnglesIllum.AngleStartOnPlane || angleStart.Item2 > StartAnglesIllum.AngleEndOnPlane || angleStart.Item2.IsEqual(angleEnd.Item2, 0.001))
                return null;

            if (angleStart.Item2 < StartAnglesIllum.AngleStartOnPlane)
            {
                var ptStart = IllumAreaBase.GetPointInRayFromPoint(ptCalc2d, angleStart.Item1, StartAnglesIllum.AngleStartOnPlane);
                angleStart = new Tuple<Point2d, double>(ptStart, StartAnglesIllum.AngleStartOnPlane);
            }
            if (angleEnd.Item2 > StartAnglesIllum.AngleEndOnPlane)
            {
                var ptEnd = IllumAreaBase.GetPointInRayFromPoint(ptCalc2d, angleEnd.Item1, StartAnglesIllum.AngleEndOnPlane);
                angleEnd = new Tuple<Point2d, double>(ptEnd, StartAnglesIllum.AngleEndOnPlane);
            }
            var ilum = new IllumAreaCentral(ptCalc2d, angleStart.Item2, angleEnd.Item2, angleStart.Item1, angleEnd.Item1);
            return ilum;
        }

        /// <summary>
        /// Расчетная область - от точки
        /// </summary>        
        private Extents3d GetCalcExtents (double maxHeight)
        {
            // высота тени - отступ по земле от расчетной точки до линии движения тени
            double cSunPlane;
            double ySunPlane = values.YShadowLineByHeight(maxHeight, out cSunPlane);
            // растояние до точки пересечения луча и линии тени
            double xRayToStart = values.GetXRay(ySunPlane, StartAnglesIllum.AngleStartOnPlane);
            double xRayToEnd = values.GetXRay(ySunPlane, StartAnglesIllum.AngleEndOnPlane);
            Extents3d ext = new Extents3d(new Point3d(ptCalc.X + xRayToEnd, ptCalc.Y - ySunPlane, 0),
                                          new Point3d(ptCalc.X + xRayToStart, ptCalc.Y, 0));
            return ext;
        }

        private bool DefineStartAnglesByOwnerBuilding ()
        {
            if (!WithOwnerBuilding) return true;

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

            //buildingOwner?.InitContour();
            //using (var contour = buildingOwner?.Contour)
            //{
                // Ограничения от собственного сегмента и окна
                CorrectStartAnglesByOwnerSegAndWindow();

                // Проверка ограничения от самого здания - горизонтальная линия через расчетную точку
                List<Point3d> ptsIntersectShadow;
                if (!CorrectStartAnglesByOwnerZeroHorLineIntersects(out ptsIntersectShadow))
                {
                    // Не все стороны определены от горизоентальной линии - построение вертикальной линии через расчетную точку
                    //CorrectStartAnglesByOwnerZeroVerticLineIntersects(ptsIntersectShadow);
                }
            //}

            // Стартовый угол не может быть больше конечного
            if (IsAllShadow()) return false;
            // Конечный угол не может быть больше Pi
            if (StartAnglesIllum.AngleEndOnPlane >= Math.PI)
            {
                throw new Exception("Ошибка расчета ограничивающих углов.");
            }
            return true;
        }        

        /// <summary>
        /// Определение стартовых углов от собственной плоскости окна с учетом отступа
        /// </summary>        
        private void CorrectStartAnglesByOwnerSegAndWindow ()
        {            
            var contour = buildingOwner.Contour;
            var startParam = contour.GetParameterAtPoint(ptCalc);

            // Если параметр близок к целому числу - то это вершина - угол! - окно не учитывается!?
            if (startParam.IsWholeNumber(0.01))
            {
                // Углы не считать!
                throw new Exception("Угол не считается.");
                //CorrectStartAnglesByOwnerCorner(Convert.ToInt32(startParam));
            }
            else
            {
                var segStartIndex = (int)startParam;
                var segOwner = contour.GetLineSegment2dAt(segStartIndex);

                // Вектор перпендикуляроно выходящий из окна во вне дома
                var vecWindPerp = GetWindowPerpVector(segOwner, contour);
                // Восточный угол = повороту перп вектороа окна на 90 (по автокадовски)
                var eastVec = vecWindPerp.RotateBy(MathExt.PIHalf);
                var eastAngleAcad = eastVec.Angle;
                var westAngleAcad = eastVec.Angle + Math.PI;

                // Учет теневого угла окна
                if (insPt.Window != null)
                {
                    eastAngleAcad -= insPt.Window.ShadowAngle;
                    westAngleAcad += insPt.Window.ShadowAngle;
                }

                var eastAngle = values.GetInsAngleFromAcad(eastAngleAcad);                
                var westAngle = values.GetInsAngleFromAcad(westAngleAcad);

                // Корректировка стартовых углов                
                CorrectEastStartAngle(eastAngle);                
                CorrectWestStartAngle(westAngle);
            }            
        }

        private void CorrectWestStartAngle (double westAngle)
        {
            if (westAngle < StartAnglesIllum.AngleEndOnPlane)                
            {
                StartAnglesIllum.AngleEndOnPlane = westAngle;
            }
        }

        private void CorrectEastStartAngle (double eastAngle)
        {
            if (eastAngle > StartAnglesIllum.AngleStartOnPlane &&
                eastAngle <= StartAnglesIllum.AngleStartOnPlane + Math.PI)
            {
                StartAnglesIllum.AngleStartOnPlane = eastAngle;
            }
        }

        /// <summary>
        /// Корректировка стартовых углов инсоляции от расчетной точки которая находится на вершине полилинии контура здания
        /// </summary>
        /// <param name="vertexIndex">Индекс вершины - расчетной точки</param>
        private void CorrectStartAnglesByOwnerCorner (int vertexIndex)
        {
            var contour = buildingOwner.Contour;
            var nextIndex = contour.NextVertexIndex(vertexIndex, -1);
            var seg = contour.GetLineSegment2dAt(vertexIndex);
            var segNext = contour.GetLineSegment2dAt(nextIndex);
            // Область освещения от угла контура - от 1 сегмента до 2 (угол вне дома)
            var cornerIllum = new IllumAreaBase(ptCalc2d, seg.Direction.Angle, segNext.Direction.Negate().Angle, Point2d.Origin, Point2d.Origin);

            // Проверка - если средний вектор внутри здания, то инвертировать область
            var midVec = cornerIllum.GetMidVector();
            var ptMid = ptCalc2d + midVec;
            if (!contour.IsPointInsidePolygon(ptMid.Convert3d()))
            {
                // инвертировать область
                cornerIllum.Invert();
            }

            var eastAngle = values.GetInsAngleFromAcad(cornerIllum.AngleStartOnPlane);
            CorrectEastStartAngle(eastAngle);
            CorrectWestStartAngle(values.GetInsAngleFromAcad(cornerIllum.AngleEndOnPlane));
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
            return StartAnglesIllum.AngleStartOnPlane >= StartAnglesIllum.AngleEndOnPlane;
        }

        private bool CorrectStartAnglesByOwnerZeroHorLineIntersects (out List<Point3d> ptsIntersectShadow)
        {
            ptsIntersectShadow = new List<Point3d> ();
            bool isCorrect = true;
            var contour = buildingOwner.Contour;
            // Линия через точку ноль - Горизонтально
            using (var lineZero = new Line(ptCalc, new Point3d(ptCalc.X + 1, ptCalc.Y, 0)))
            {
                List<Point3d> ptsIntersectZero;
                using (var ptsIntersect = new Point3dCollection())
                {
                    contour.IntersectWith(lineZero, Intersect.ExtendArgument, new Plane(), ptsIntersect, IntPtr.Zero, IntPtr.Zero);
                    ptsIntersectZero = ptsIntersect.Cast<Point3d>().OrderBy(o => o.X).ToList();
                }

                // Линия тени - пересечение собственного дрма сней?                
                using (var lineShadow = GetLineShadow(buildingOwner.HeightCalc))
                {
                    using (var ptsIntersect = new Point3dCollection())
                    {
                        contour.IntersectWith(lineShadow, Intersect.OnBothOperands, new Plane(), ptsIntersect, IntPtr.Zero, IntPtr.Zero);
                        ptsIntersectShadow = ptsIntersect.Cast<Point3d>().ToList();
                    }
                }

                // Разделить точки левее стартовой и правее (Запад/Восток) - для каждой петли свойестороны найти максимальный ограничивающий угол.                    
                double westAngle;
                if (GetStartAngleBySideIntersects(contour, ptsIntersectZero, ptsIntersectShadow, false, true, false, out westAngle))
                {
                    if (westAngle < StartAnglesIllum.AngleEndOnPlane)
                    {
                        StartAnglesIllum.AngleEndOnPlane = westAngle;
                    }
                }
                else
                    isCorrect = false;

                double eastAngle;
                if (GetStartAngleBySideIntersects(contour, ptsIntersectZero, ptsIntersectShadow, true, true, false, out eastAngle))
                {
                    if (eastAngle > StartAnglesIllum.AngleStartOnPlane)
                    {
                        StartAnglesIllum.AngleStartOnPlane = eastAngle;
                    }
                }
                else
                    isCorrect = false;
            }
            return isCorrect;
        }

        /// <summary>
        /// Корректировка стартовых углов от собственного дома - построение вертикальной линии сечения из расчетной точки
        /// </summary>
        private void CorrectStartAnglesByOwnerZeroVerticLineIntersects (List<Point3d> ptsIntersectShadow)
        {
            // Построение вертик линии через расч.точку
            // Найти точки пересечения ниже расетной точки
            // Взять петлю слева (Запад) и справа (Восток)

            using (var lineVertic = new Line(ptCalc, new Point3d (ptCalc.X, ptCalc.Y-1, 0)))
            {
                var contour = buildingOwner.Contour;

                // Точки пересечения контура с вертик линей - ниже или на уровне с расчетной точкой! Отсортированных по Y сверху - вниз
                List<Point3d> ptsIntersectVertic;
                using (var ptsIntersect = new Point3dCollection())
                {
                    contour.IntersectWith(lineVertic, Intersect.ExtendArgument, new Plane(), ptsIntersect, IntPtr.Zero, IntPtr.Zero);
                    ptsIntersectVertic = ptsIntersect.Cast<Point3d>().Where(p=>p.Y<=ptCalc.Y).OrderByDescending(o => o.Y).ToList();
                }               

                // Найти петли с левой и правой стороны от точек пересечения                
                double westAngle;
                if (GetStartAngleBySideIntersects(contour, ptsIntersectVertic, ptsIntersectShadow, false, false, true, out westAngle))
                {
                    if (westAngle < StartAnglesIllum.AngleEndOnPlane)
                    {
                        StartAnglesIllum.AngleEndOnPlane = westAngle;
                    }
                }                

                double eastAngle;
                if (GetStartAngleBySideIntersects(contour, ptsIntersectVertic, ptsIntersectShadow, true, false, true, out eastAngle))
                {
                    if (eastAngle > StartAnglesIllum.AngleStartOnPlane)
                    {
                        StartAnglesIllum.AngleStartOnPlane = eastAngle;
                    }
                }
            }
        }

        /// <summary>
        /// Корректировка стартового угла ограницения от дома - по точкам пересечения полилинии с горизонталью
        /// </summary>
        /// <param name="contour">Контур</param>
        /// <param name="ptsIntersect">все точки пересечения, отсортированные по X</param>
        /// <param name="ptsIntersectShadow">Точки пересечения дома с линией тени</param>
        /// <param name="isEastSide">Сторона восток - запад</param>
        private bool GetStartAngleBySideIntersects (Polyline contour, List<Point3d> ptsIntersect,
            List<Point3d> ptsIntersectShadow, bool isEastSide, bool isHor, bool onlyFirstLoop, out double resAngle)
        {
            resAngle = 0;            
            if (ptsIntersect.Count == 0) return false;
            bool resIsCorrected = false;
            List<Point3d> ptsIntersectSide;
            bool maxOrMinAngle;
            if (isHor)
            {
                if (isEastSide)
                {
                    ptsIntersectSide = ptsIntersect.Where(p => p.X >= ptCalc.X).ToList();
                    maxOrMinAngle = true;
                }
                else
                {
                    ptsIntersectSide = ptsIntersect.Where(p => p.X <= ptCalc.X).ToList();
                    maxOrMinAngle = false;
                    resAngle = Math.PI;
                }
            }
            else
            {
                ptsIntersectSide = ptsIntersect;
                maxOrMinAngle = isEastSide;
            }

            if (!ptsIntersectSide.Skip(1).Any())
            {
                return false;
            }

            var ptPrew = ptsIntersectSide[0];
            foreach (var pt in ptsIntersectSide.Skip(1))
            {
                // средняя точка должна быть внутри полилинии                
                var ptCentre = ptPrew + (pt- ptPrew)/2;                
                if (contour.IsPointInsidePolygon(ptCentre) && !contour.IsPointOnPolyline(ptCentre))
                {
                    // Петля полилинии ниже заданных точек                    
                    var ptsLoopBelow = isHor? contour.GetLoopSideBetweenHorizontalIntersectPoints(ptPrew, pt, false, true):
                                              contour.GetLoopSideBetweenVerticalIntersectPoints(ptPrew, pt, false, false);
                    if (ptsIntersectShadow.Count > 0 && ptsLoopBelow.Count > 0)
                    {
                        // Проверка точек пересечения с линией тени - добавление этих точек
                        var y = ptsIntersectShadow[0].Y;   
                        using (var plLoop = ptsLoopBelow.CreatePolyline())
                        {
                            ptsLoopBelow = ptsLoopBelow.Where(p => p.Y >= y).ToList();
                            foreach (Point3d ptLSh in ptsIntersectShadow)
                            {
                                if (plLoop.IsPointOnPolyline(ptLSh))
                                {
                                    ptsLoopBelow.Add(ptLSh.Convert2d());
                                }
                            }
                        }
                    }
                    // Определение угла для каждой точки
                    foreach (var ptLoop in ptsLoopBelow)
                    {
                        if (ptLoop.Y < ptCalc.Y)
                        {
                            var angleInsPt = GetInsAngleFromPoint(ptLoop);
                            if ((maxOrMinAngle && (angleInsPt > resAngle)) ||
                                (!maxOrMinAngle && (angleInsPt < resAngle)))
                            {
                                resAngle = angleInsPt;
                                resIsCorrected = true;
                            }
                        }
                    }
                    if (onlyFirstLoop)
                        break;
                }
                ptPrew = pt;
            }        
            return resIsCorrected;
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

        private Line GetLineShadow (double height)
        {
            // катет тени и гипотенуза тени (относительно расчетной точки) - высота линии тени
            double cShadow;
            double yShadowLen = values.YShadowLineByHeight(height, out cShadow);
            double yShadow = ptCalc2d.Y - yShadowLen;

            // Линия тени
            var xRayToStart = values.GetXRay(yShadowLen, StartAnglesIllum.AngleStartOnPlane);
            var xRayToEnd = values.GetXRay(yShadowLen, StartAnglesIllum.AngleEndOnPlane);
            Line lineShadow = new Line(new Point3d(ptCalc.X + xRayToStart, yShadow, 0),
                              new Point3d(ptCalc.X + xRayToEnd, yShadow, 0));
            return lineShadow;
        }
    }
}
