using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;

namespace PIK_GP_Acad.Insolation.Services
{
    public static class VisualHelper
    {
        private const string textStyleName = "Insolation";
        private const string textStyleFontFile = "romans.shx";

        public static ObjectId GetTextStyleId (Document doc)
        {            
            var db = doc.Database;
            ObjectId res = db.Textstyle;

            //using (doc.LockDocument())
            using (var t = db.TransactionManager.StartTransaction())
            {
                var textStyleTable = db.TextStyleTableId.GetObject(OpenMode.ForRead) as TextStyleTable;
                if (textStyleTable.Has(textStyleName))
                {
                    res = textStyleTable[textStyleName];
                }
                else
                {
                    textStyleTable.UpgradeOpen();
                    var insTextStyle = new TextStyleTableRecord();
                    insTextStyle.Name = textStyleName;
                    res = textStyleTable.Add(insTextStyle);
                    t.AddNewlyCreatedDBObject(insTextStyle, true);
                }
                CheckTextStyle(res);

                t.Commit();
            }
            return res;
        }

        public static Autodesk.AutoCAD.DatabaseServices.Polyline CreatePolyline (List<Point2d> points, VisualOption opt)
        {
            Point2d[] pts = DistincPoints(points);
            var pl = new Autodesk.AutoCAD.DatabaseServices.Polyline();
            for (int i = 0; i < pts.Length; i++)
            {
                pl.AddVertexAt(i, pts[i], 0, 0, 0);
            }
            pl.Closed = true;
            SetEntityOpt(pl, opt);
            return pl;
        }

        public static Hatch CreateHatch (List<Point2d> points, VisualOption opt)
        {
            Point2d[] pts = DistincPoints(points);
            // Штриховка            
            var ptCol = new Point2dCollection(pts);
            ptCol.Add(points[0]);
            var dCol = new DoubleCollection(new double[points.Count]);

            var h = new Hatch();
            h.SetHatchPattern(HatchPatternType.PreDefined, "SOLID");
            SetEntityOpt(h, opt);
            h.AppendLoop(HatchLoopTypes.Default, ptCol, dCol);
            return h;
        }

        private static Point2d[] DistincPoints (List<Point2d> points)
        {
            //  Отсеивание одинаковых точек
            return points.Distinct(new AcadLib.Comparers.Point2dEqualityComparer()).ToArray();
        }

        public static DBText CreateText (string text, VisualOption opt, double height, AttachmentPoint justify)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            DBText dbText = new DBText();
            //dbText.SetDatabaseDefaults(db);

            SetEntityOpt(dbText, opt);
            dbText.TextStyleId = GetTextStyleId(doc);
            dbText.Position = opt.Position;            
            dbText.TextString = text;            
            dbText.Height = height;
            dbText.Justify = justify;
            dbText.AlignmentPoint = opt.Position;
            dbText.AdjustAlignment(db);
            return dbText;
        }

        public static Circle CreateCircle (double radius, VisualOption opt)
        {
            Circle c = new Circle(opt.Position, Vector3d.ZAxis, radius);
            SetEntityOpt(c, opt);
            return c;
        }

        public static void SetEntityOpt(Entity ent, VisualOption opt)
        {
            if (opt.Color != null)
                ent.Color = opt.Color;
            if (opt.Transparency.Alpha != 0)
                ent.Transparency = opt.Transparency;            
        }

        /// <summary>
        /// Проверка свойств текстового стиля для визуализации
        /// </summary>
        /// <param name="insTextStyleId">Тестовый стиль инсоляции</param>
        private static void CheckTextStyle (ObjectId insTextStyleId)
        {
            var insTextStyle = insTextStyleId.GetObject(OpenMode.ForRead) as TextStyleTableRecord;

            // Шрифт
            if (!insTextStyle.FileName.Equals(textStyleFontFile, StringComparison.OrdinalIgnoreCase))
            {
                insTextStyle.UpgradeOpen();
                insTextStyle.FileName = textStyleFontFile;
            }
        }
    }
}
