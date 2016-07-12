using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.Insolation.SunlightRule;
using AcadLib;
using AcadLib.Errors;

namespace PIK_GP_Acad.Insolation
{
    /// <summary>
    /// Радар - расчета инсоляции в точке
    /// </summary>
    public class Radar
    {
        Options options;
        ISunlightRule rule;
        Database db;

        public Radar (Database db, Options options)
        {
            this.db = db;
            this.options = options;
            rule = options.SunlightRule;
        }

        /// <summary>
        /// Сканирование области из точки
        /// </summary>        
        public List<IlluminationArea> Scan (Point3d pt, Scope scope, BlockTableRecord ms)
        {
            // найти точки пересечения с объектами по направлению                                    
            var illuminations = new List<IlluminationArea>();
            var plBuildings = scope.Buildings.Select(b => b.GetCurve()).ToList();            

            using (Line ray = new Line(pt, new Point3d(pt.X + scope.Radius, pt.Y, 0)))
            {
                // Поворот луча в начальный угол
                RotateRay(ray, rule.StartAngle);

                // Сканирование с крупным шагом, до обнаружения препятствия
                int countLargeStep = Convert.ToInt32((rule.EndAngle - rule.StartAngle) / options.ScaningStepLarge);

                IlluminationArea illumArea = null;                
                bool scanToStartIllumArea = true;
                Point3d ptIntersectNearest = Point3d.Origin;

                for (double i = 0; i < countLargeStep; i += options.ScaningStepLarge)
                {
                    Point3d ptFound;  
                    if (FindIntersectNearestPiont(plBuildings, ray, out ptFound))
                    {
                        ptIntersectNearest = ptFound;
                        // Найдено пересечение луча со зданием
                        if (!scanToStartIllumArea)
                        {
                            // Конец освещенного участка
                            illumArea.EndPoint = CorrectScan(ray, plBuildings, true);
                            illuminations.Add(illumArea);                            
                            illumArea = null;
                            scanToStartIllumArea = true;
                        }
                    }
                    else
                    {
                        // Не найдено пересечений

                        if (scanToStartIllumArea)
                        {
                            // Начало освещенного участка
                            illumArea = new IlluminationArea(options, rule, db, ray.StartPoint);
                            if (ptIntersectNearest == Point3d.Origin)
                            {
                                illumArea.StartPoint = ray.EndPoint;
                            }
                            else
                            {
                                // найти точку пересечения
                                illumArea.StartPoint = CorrectScan(ray, plBuildings, false);
                            }
                            scanToStartIllumArea = false;
                        }
                    }

                    RotateRay(ray, options.ScaningStepLarge);                    
                }

                // Если есть начатая освещенная зона - добавить ее
                if (illumArea!= null)
                {
                    illumArea.EndPoint = ray.EndPoint;
                    illuminations.Add(illumArea);
                }
            }
            return illuminations;
        }

        /// <summary>
        /// Поворот луча на заданный угол в градусах
        /// </summary>        
        private void RotateRay (Line ray, double angle)
        {
            ray.TransformBy(Matrix3d.Rotation(-angle.ToRadians(), Vector3d.ZAxis, ray.StartPoint));
        }

        private static bool FindIntersectNearestPiont (IEnumerable<Polyline> plBuildings, Line ray, 
            out Point3d ptIntersectNearest)
        {
            bool findRes = false;
            ptIntersectNearest = Point3d.Origin;
            foreach (var plBuild in plBuildings)
            {
                var ptIntersects = new Point3dCollection();
                ray.IntersectWith(plBuild, Intersect.OnBothOperands, new Plane(), ptIntersects, IntPtr.Zero, IntPtr.Zero);
                if (ptIntersects.Count > 0)
                {
                    var ptNearest = NearestPoint(ptIntersects, ray.StartPoint);
                    if (findRes)
                    {
                        if ((ptIntersectNearest - ray.StartPoint).Length > (ptNearest - ray.StartPoint).Length)
                        {
                            ptIntersectNearest = ptNearest;
                        }
                        findRes = true;
                    }
                    else
                    {
                        ptIntersectNearest = ptNearest;
                        findRes = true;
                    }
                }
            }
            return findRes;
        }

        private Point3d CorrectScan (Line ray, IEnumerable<Polyline> plBuildings, bool wasIntersect)
        {
            Point3d ptRes = Point3d.Origin;            
            int clockwiseFactor = -1;
            var rayCorrect = (Line)ray.Clone();
            bool repeat = false;
            bool defineRes = false;        
            do
            {
                Point3d ptIntersect;     
                if (FindIntersectNearestPiont(plBuildings, rayCorrect, out ptIntersect))
                {
                    // Найдено пересечение
                    if (wasIntersect)
                    {
                        // Если изначально было пересечение, то надо найти пустую область без пересечений, тогда точка последнего пересечения будет результатом
                        repeat = true;
                        ptRes = ptIntersect;
                    }
                    else
                    {
                        // Если изначально не было пересечения, то результат определен
                        repeat = false;
                        ptRes = ptIntersect;
                        defineRes = true;
                    }
                }
                else
                {
                    // Пересечение не найдено
                    if (wasIntersect)
                    {                        
                        repeat = false;
                        defineRes = true;
                    }
                    else
                    {
                        // Если изначально не было пересечения, то продолжаем поиск пересечения
                        repeat = true;
                    }
                }
                // поворот луча
                RotateRay(rayCorrect, options.ScaningStepSmall * clockwiseFactor);
                
            } while (repeat);
            
            if (!defineRes)
            {
                throw new InvalidOperationException($"Недопустимая ситуация - не определено точное пересечение.");
            }

            return ptRes;
        }

        private static Point3d NearestPoint (Point3dCollection pts, Point3d ptDest)
        {
            Point3d ptRes;
            var ptsArray = new Point3d[pts.Count];
            pts.CopyTo(ptsArray, 0);
            ptRes = ptsArray.OrderBy(p => (p - ptDest).Length).First();
            return ptRes;
        }
    }
}
