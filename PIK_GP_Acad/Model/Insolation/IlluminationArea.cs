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
        private InsOptions options;
        private ISunlightRule rule;

        public Point3d Origin { get; set; }
        public Point3d EndPoint { get; set; }
        public Point3d StartPoint { get; set; }

        public IlluminationArea (InsOptions options, ISunlightRule rule, Database db, Point3d origin)
        {
            Origin = origin;
            this.options = options;
            this.rule = rule;
            this.db = db;
        }

        /// <summary>
        /// Построение контура освещенности
        /// </summary>        
        public void Create (BlockTableRecord cs)
        {
            Point3d ptLewelEndW;
            Point3d ptLewelEndH;
            // Низкоэтажный уровень
            createPlByLevel(cs, Origin, Origin, options.LowHeight, options.LowHeightColor,out ptLewelEndW,out ptLewelEndH);
            // Средний уровень
            createPlByLevel(cs, ptLewelEndH, ptLewelEndW, options.MediumHeight, options.MediumHeightColor,
                out ptLewelEndW, out ptLewelEndH);
            // Высотный уровень
            createPlByLevel(cs, ptLewelEndH, ptLewelEndW, options.MaxHeight, options.MaxHeightColor,
                out ptLewelEndW, out ptLewelEndH);
        }

        private void createPlByLevel (BlockTableRecord cs, Point3d ptStartH, Point3d ptStartW, int level, Color color, 
            out Point3d ptLewelEndW, out Point3d ptLewelEndH)
        {
            Transaction t = cs.Database.TransactionManager.TopTransaction;
            ptLewelEndW = rule.GetPointByHeightInVector(Origin,
                            StartPoint.Convert2d() - Origin.Convert2d(), level);
            ptLewelEndH = rule.GetPointByHeightInVector(Origin,
                EndPoint.Convert2d() - Origin.Convert2d(), level);

            Point3dCollection pts = new Point3dCollection(new Point3d[]{
                                    ptStartH, ptStartW, ptLewelEndW, ptLewelEndH
                                });
            Polyline3d pl3d = new Polyline3d(Poly3dType.SimplePoly, pts, true);
            pl3d.Color = color;
            pl3d.Transparency = new Transparency(options.Transparence);
            cs.AppendEntity(pl3d);
            t.AddNewlyCreatedDBObject(pl3d, true);
            var h = AcadLib.Hatches.HatchExt.CreateAssociativeHatch(pl3d, cs, t);
            if (h != null)
            {
                h.Color = color;
                h.Transparency = new Transparency(options.Transparence);
            }
        }
    }
}
