using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.Insolation.SunlightRule;
using AcadLib;

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
        public void Scan (Point3d pt, Scope scope, BlockTableRecord ms)
        {
            // найти точки пересечения с объектами по направлению                        
            Plane plane = new Plane ();
            List<Point3d> ptIntersects = new List<Point3d>();            
            List<Polyline> pls = new List<Polyline> ();
            foreach (var item in scope.Buildings)
            {
                pls.Add(item.GetCurve());
            }

            using (Line ray = new Line(pt, new Point3d(pt.X + scope.Radius, pt.Y, 0)))
            {
                for (double i = 0; i < 1800; i+=0.1)
                {
                    ray.TransformBy(Matrix3d.Rotation(MathExt.ToRadians(0.1), Vector3d.ZAxis, pt));                    
                    foreach (var curve in pls)
                    {                        
                        Point3dCollection pts = new Point3dCollection ();
                        ray.IntersectWith(curve, Intersect.OnBothOperands, plane, pts, IntPtr.Zero, IntPtr.Zero);
                        if (pts.Count > 0)
                        {
                            // Берем ближайшую точку к чентру
                            Point3d[] ptsCopy = new Point3d[pts.Count];
                            pts.CopyTo(ptsCopy, 0);
                            var PtIntersectNearest = ptsCopy.OrderBy(p => (p - pt).Length).First();
                            ptIntersects.Add(PtIntersectNearest);
                        }
                    }
                }
            }
            // Рисование линий до найденных точек пересечения из центра
            foreach (var item in ptIntersects)
            {
                Line line = new Line (pt, item);
                line.SetDatabaseDefaults(db);
                ms.AppendEntity(line);
                db.TransactionManager.TopTransaction.AddNewlyCreatedDBObject(line, true);
            }
        }
    }
}
