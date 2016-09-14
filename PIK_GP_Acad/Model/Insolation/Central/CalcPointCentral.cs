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

namespace PIK_GP_Acad.Insolation.Central
{
    class CalcPointCentral
    {
        Point3d ptCalc;
        Point2d ptCalc2d;
        IInsolationService insService;
        int maxHeight;
        CalcValuesCentral values;
        /// <summary>
        /// Начальный угол в плане (радиан). Начальное значение = 0 - восход.
        /// Будут определены для этой расвчетной точки индивидуально
        /// </summary>
        double angleStartOnPlane;
        /// <summary>
        /// Конечный угол в плане (радиан). Начальное значение = 180 - заход
        /// </summary>
        double angleEndOnPlane;
        Plane plane = new Plane();

        public CalcPointCentral (Point3d pt, IInsolationService insService)
        {
            this.ptCalc = pt;
            ptCalc2d = pt.Convert2d();
            this.insService = insService;
            maxHeight = insService.Options.HeightMax;
            values = insService.CalcValues;

            angleStartOnPlane = values.SunCalcAngleStartOnPlane;
            angleEndOnPlane = values.SunCalcAngleEndOnPlane;            
        }
        
        public List<IIlluminationArea> Calc ()
        {
            var resAreas = new List<IIlluminationArea>();
            
            // расчетный дом
            var buildingOwner = GetOwnerBuilding();

            // Корректировка расчетной точки
            CorrectCalcPoint(buildingOwner);

            // Определение ограничений углов (начального и конечного) с учетом плоскости стены расчетного дома
            DefineAnglesStartEndByOwnerBuilding(buildingOwner);

            // расчетные граници (по заданным расчетным углам)
            var ext = GetCalcExtents();

            // кусок карты
            var scope = insService.Map.GetScope(ext);            
            // исключение из списка домов собственно расчетного дома
            scope.Buildings.Remove(buildingOwner);            

            // группировка домов по высоте
            var heights = scope.Buildings.GroupBy(g => g.Height);
            foreach (var bHeight in heights)
            {
                // зоны освещенности для домов этой высоты
                var illumsByHeight = CalcIllumsByHeight(bHeight.ToList(), bHeight.Key);                
                resAreas.AddRange(illumsByHeight);
            }
            resAreas = IlluminationArea.Merge(resAreas);

            return resAreas;
        }        

        private List<IIlluminationArea> CalcIllumsByHeight(List<InsBuilding> buildings, int height)
        {
            List<IIlluminationArea> illumShadows = new List<IIlluminationArea>();

            // катет тени и гипотенуза тени (относительно расчетной точки) - высота линии тени
            double cShadow;
            double yShadowLen = values.YShadowLineByHeight(height, out cShadow);
            double yShadow = ptCalc2d.Y - yShadowLen;

            // Линия тени
            var xRayToStart = values.GetXRay(yShadowLen, angleStartOnPlane);
            var xRayToEnd = values.GetXRay(yShadowLen, angleEndOnPlane);
            Line lineShadow = new Line(new Point3d(ptCalc.X + xRayToStart, yShadow, 0),
                              new Point3d(ptCalc.X + xRayToEnd, yShadow, 0));            
            // перебор домов одной высоты
            foreach (var build in buildings)
            {
                // Если дом полностью выше линии тени (сечения), то он полностью затеняет точку
                if (build.YMin > yShadow)
                {
                    // Найти точку начала тени и конца (с минимальным и макс углом к точке расчета)
                    var ilumShadow = GetIllumShadow(build.Contour.GetPoints(), true);
                    if (ilumShadow != null)
                    {
                        illumShadows.Add(ilumShadow);
                    }
                }
                else if(build.YMax > yShadow)
                {
                    var ilumsBoundary = GetBuildingLineShadowBoundary(build, lineShadow, true);
                    illumShadows.AddRange(ilumsBoundary);
                }
            }
            // Объединение совпадающих зон теней
            illumShadows = IlluminationArea.Merge(illumShadows);
            return illumShadows;
        }

        private List<IIlluminationArea> GetBuildingLineShadowBoundary (InsBuilding build,Line lineShadow, bool above)
        {
            List<IIlluminationArea> resIlumsShadows = new List<IIlluminationArea>();
            // Дом на границе тени, нужно строить линию пересечения с домом
            Point3dCollection ptsIntersects = new Point3dCollection();
            lineShadow.IntersectWith(build.Contour, Intersect.ExtendThis, plane, ptsIntersects, IntPtr.Zero, IntPtr.Zero);
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
                    var points = GetLoopPointsAbove(build.Contour, pt1, pt, above);

                    var ilumShadow = GetIllumShadow(points, false);

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
        /// Найти точки петли полилинии выше точек сечения
        /// </summary>
        /// <param name="contour">Исходная Полилиния</param>
        /// <param name="pt1">Первая точка петли (пересечения)</param>
        /// <param name="pt2">Вторая точка петли (пересечения)</param>
        /// <returns>Список точек петли выше пересечения (включая точки пересечения)</returns>
        private List<Point2d> GetLoopPointsAbove (Polyline contour, Point2d pt1, Point2d pt2, bool above)
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
            if (above ? seg.StartPoint.Y > seg.EndPoint.Y : seg.StartPoint.Y < seg.EndPoint.Y)
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
                isContinue = above? pt.Y > pt2.Y : pt.Y < pt2.Y;
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
        /// Расчетная область - от точки
        /// </summary>        
        private Extents3d GetCalcExtents ()
        {
            // высота тени - отступ по земле от расчетной точки до линии движения тени
            double cSunPlane;
            double ySunPlane = values.YShadowLineByHeight(maxHeight, out cSunPlane);
            // растояние до точки пересечения луча и линии тени
            double xRayToStart = values.GetXRay(ySunPlane, angleStartOnPlane);
            double xRayToEnd = values.GetXRay(ySunPlane, angleEndOnPlane);
            Extents3d ext = new Extents3d(new Point3d (ptCalc.X+ xRayToEnd, ptCalc.Y-ySunPlane,0),
                                          new Point3d (ptCalc.X + xRayToStart, ptCalc.Y, 0));            
            return ext;
        }

        private InsBuilding GetOwnerBuilding ()
        {
            var buildingOwner = insService.Map.GetBuildingInPoint(ptCalc);
            if (buildingOwner== null)
            {
                throw new Exception($"Не определен дом принадлежащий расчетной точке.");
            }
            return buildingOwner;
        }

        private void DefineAnglesStartEndByOwnerBuilding (InsBuilding buildingOwner)
        {            
            // Линия сечения здания проходящая через расчетную точку (горизонтально)
            Line lineShadowZero = new Line(new Point3d (buildingOwner.ExtentsInModel.MinPoint.X, ptCalc.Y, 0),
                                    new Point3d (buildingOwner.ExtentsInModel.MaxPoint.X, ptCalc.Y, 0));
            var ilumsBoundary = GetBuildingLineShadowBoundary(buildingOwner, lineShadowZero, false);
            if (ilumsBoundary.Count != 0)
            {
                if (ilumsBoundary[0].AngleEndOnPlane> angleEndOnPlane)
                {
                    angleEndOnPlane = ilumsBoundary[0].AngleStartOnPlane;
                }
                else
                {                    
                    if (ilumsBoundary.Count>1)
                    {
                        var lastIlum = ilumsBoundary.Last();
                        if (lastIlum.AngleStartOnPlane< angleEndOnPlane)
                        {
                            angleEndOnPlane = lastIlum.AngleStartOnPlane;
                        }
                    }
                    else
                    {
                        if (ilumsBoundary[0].AngleStartOnPlane <= angleStartOnPlane)
                        {
                            angleStartOnPlane = ilumsBoundary[0].AngleEndOnPlane;
                        }
                        else if (ilumsBoundary[0].AngleEndOnPlane>= angleEndOnPlane)
                        {
                            angleEndOnPlane = ilumsBoundary[0].AngleStartOnPlane;
                        }
                    }
                }
            }
        }

        private void CorrectCalcPoint (InsBuilding buildingOwner)
        {   
            var correctPt = buildingOwner.Contour.GetClosestPointTo(ptCalc, true);
            ptCalc = correctPt;
            ptCalc2d = correctPt.Convert2d();            
        }

        /// <summary>
        /// Определение граничных точек задающих тень по отношению к расчетной точке, среди всех точек объекта
        /// </summary>        
        private IIlluminationArea GetIllumShadow (List<Point2d> points, bool above)
        {
            IIlluminationArea ilum = null;
            // список точек и их углов к расчетной точке
            List<double> angles = new List<double>();
            foreach (var iPt in points)
            {
                // угол к расчетной точке (от 0 по часовой стрелке)
                if ((!above || ptCalc.Y - iPt.Y > 1) && !ptCalc2d.IsEqualTo(iPt))
                {
                    var angle = Math.PI - (ptCalc2d - iPt).Angle;
                    angles.Add(angle);
                }
            }
            if (angles.Count > 1)
            {
                angles.Sort();
                ilum = CreateIllumShadow(angles[0], angles[angles.Count - 1]);
            }
            return ilum;
        }

        private IIlluminationArea CreateIllumShadow (double angleStart, double angleEnd)
        {
            // если конечный угол меньше начального расчетного или наоборот, то тень вне границ расчета. Или если начальный угол = конечному
            if (angleEnd < angleStartOnPlane || angleStart > angleEndOnPlane ||angleStart.IsEqual(angleEnd, 0.001))
                return null;
            if (angleStart < angleStartOnPlane)
                angleStart = angleStartOnPlane;
            if (angleEnd > angleEndOnPlane)
                angleEnd = angleEndOnPlane;
            var ilum = new CentralIllumArea(insService, angleStart, angleEnd, ptCalc2d);
            return ilum;
        }
    }
}
