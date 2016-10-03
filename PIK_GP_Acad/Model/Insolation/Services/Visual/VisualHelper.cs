using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;

namespace PIK_GP_Acad.Insolation.Services
{
    public static class VisualHelper
    {
        public static Hatch CreateHatch (List<Point2d> points, VisualOption opt)
        {
            // Штриховка
            var ptCol = new Point2dCollection(points.ToArray());
            var dCol = new DoubleCollection(new double[points.Count]);            

            var h = new Hatch();
            h.SetHatchPattern(HatchPatternType.PreDefined, "SOLID");
            SetEntityOpt(h, opt);            
            h.AppendLoop(HatchLoopTypes.Default, ptCol, dCol);
            return h;
        }

        public static DBText CreateText (string text, VisualOption opt, double height, AttachmentPoint justify)
        {
            DBText dbText = new DBText();
            dbText.SetDatabaseDefaults(HostApplicationServices.WorkingDatabase);

            SetEntityOpt(dbText, opt);

            dbText.Position = opt.Position;            
            dbText.TextString = text;            
            dbText.Height = height;
            dbText.Justify = justify;
            dbText.AlignmentPoint = opt.Position;
            dbText.AdjustAlignment(HostApplicationServices.WorkingDatabase);
            return dbText;
        }

        public static Circle CreateCircle (double radius, VisualOption opt)
        {
            Circle c = new Circle(opt.Position, Vector3d.ZAxis, radius);
            SetEntityOpt(c, opt);
            return c;
        }

        private static void SetEntityOpt(Entity ent, VisualOption opt)
        {
            if (opt.Color != null)
                ent.Color = opt.Color;
            if (opt.Transparency.Alpha != 0)
                ent.Transparency = opt.Transparency;
        }
    }
}
