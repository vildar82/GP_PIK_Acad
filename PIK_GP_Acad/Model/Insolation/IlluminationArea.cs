using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.Insolation.SunlightRule;

namespace PIK_GP_Acad.Insolation
{
    /// <summary>
    /// Зона освещенности - контур освещенности для заданной точки определеннойц радаром
    /// </summary>
    public class IlluminationArea
    {
        private Database db;
        private Options options;
        private ISunlightRule rule;

        public Point3d Origin { get; set; }
        public Point3d EndPoint { get; set; }
        public Point3d StartPoint { get; set; }

        public IlluminationArea (Options options, ISunlightRule rule, Database db, Point3d origin)
        {
            Origin = origin;
            this.options = options;
            this.rule = rule;
            this.db = db;
        }

        /// <summary>
        /// Построение контура освещенности
        /// </summary>        
        public void Create (BlockTableRecord cs, Transaction t)
        {
            Point3dCollection pts = new Point3dCollection(new Point3d[] { Origin, StartPoint, EndPoint });
            Polyline3d pl3d = new Polyline3d(Poly3dType.SimplePoly, pts, true);
            cs.AppendEntity(pl3d);
            t.AddNewlyCreatedDBObject(pl3d, true);
            var h = AcadLib.Hatches.HatchExt.CreateAssociativeHatch(pl3d, cs, t);
            h.Color = Color.FromColor(System.Drawing.Color.Yellow);
        }
    }
}
