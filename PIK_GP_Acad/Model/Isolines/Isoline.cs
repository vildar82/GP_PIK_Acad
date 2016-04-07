using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace PIK_GP_Acad.Isolines
{
    public class Isoline
    {
        private static IsolineOptions _options;
        private static IsolineDrawableOverrule _overruleIsolineDraw = null;
        private static IsolineTransformOverrule _overruleIsolineTrans = null;

        public const string RegAppNAME = "GP-Isoline";
        private static RXClass rxCurve = RXObject.GetClass(typeof(Curve));
        private ObjectId idCurve { get; set; }
        /// <summary>
        /// Это линия бергштриха
        /// </summary>
        private bool isDash { get; set; }
        /// <summary>
        /// Это линия для бергштрихов
        /// </summary>
        public bool IsIsoline { get; private set; }
        private bool isNegate { get; set; }

        public Isoline(Curve curve)
        {
            idCurve = curve.Id;
            getIsolineProperties(curve);
        }

        public Isoline(ObjectId id)
        {
            idCurve = id;
            using (var curve = id.Open(OpenMode.ForRead, false, true) as Curve)
            {
                getIsolineProperties(curve);
            }
        }

        public static void Start()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            _options = IsolineOptions.Load();

            var optKeywords = new PromptKeywordOptions(
               $"Отрисовка бергштрихов для полилиний {(_overruleIsolineDraw == null ? "Отключена" : "Включена")}");
            optKeywords.Keywords.Add($"{(_overruleIsolineDraw == null ? "Включить" : "Отключить")}");
            optKeywords.Keywords.Add($"{(_overruleIsolineDraw == null ? "Разморозить" : "Заморозить")}");
            optKeywords.Keywords.Add("Настройки");

            var resPrompt = ed.GetKeywords(optKeywords);

            if (resPrompt.Status == PromptStatus.OK)
            {
                if (resPrompt.StringResult == "Включить")
                {
                    IsolinesOn();
                }
                else if (resPrompt.StringResult == "Отключить")
                {
                    IsolinesOff();
                }
                else if (resPrompt.StringResult == "Разморозить")
                {
                    // Удалить отдельные штрихи
                    UnfreezeAll();
                    // Включить изолинии
                    IsolinesOn();
                }
                else if (resPrompt.StringResult == "Заморозить")
                {
                    // Превратить все штрихи в отдельные линии
                    FreezeAll();
                    // выключение изолиний
                    IsolinesOff();
                }
                else if (resPrompt.StringResult == "Настройки")
                {
                    _options = _options.Show();
                }
            }
            Application.DocumentManager.MdiActiveDocument.Editor.Regen();
        }

        public static void IsolinesOff()
        {
            ContextMenuIsoline.Detach();
            if (_overruleIsolineDraw != null)
            {
                Overrule.RemoveOverrule(RXObject.GetClass(typeof(Curve)), _overruleIsolineDraw);
                _overruleIsolineDraw = null;
            }
            if (_overruleIsolineTrans != null)
            {
                Overrule.RemoveOverrule(RXObject.GetClass(typeof(Curve)), _overruleIsolineTrans);
                _overruleIsolineTrans = null;
            }
        }

        public static void IsolinesOn()
        {
            ContextMenuIsoline.Attach();
            if (_overruleIsolineDraw == null)
            {
                _overruleIsolineDraw = new IsolineDrawableOverrule();
                Overrule.AddOverrule(RXObject.GetClass(typeof(Curve)), _overruleIsolineDraw, false);
            }
            if (_overruleIsolineTrans == null)
            {
                _overruleIsolineTrans = new IsolineTransformOverrule();
                Overrule.AddOverrule(RXObject.GetClass(typeof(Curve)), _overruleIsolineTrans, false);
            }
        }

        public static void ActivateIsolines(bool activate)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            using (doc.LockDocument())
            {
                RegAppIsoline(doc.Database);
                Editor ed = doc.Editor;
                PromptSelectionResult result = ed.SelectImplied();
                if (result.Status == PromptStatus.OK)
                {
                    var selIds = result.Value.GetObjectIds();
                    if (selIds.Count() > 0)
                    {
                        using (var t = doc.Database.TransactionManager.StartTransaction())
                        {
                            foreach (var item in selIds)
                            {
                                if (item.ObjectClass.IsDerivedFrom(rxCurve))
                                {
                                    Isoline isoline = new Isoline(item);
                                    // Активация изолинии
                                    if (activate)
                                    {
                                        if (!isoline.IsIsoline)
                                        {
                                            isoline.Activate(true);
                                        }
                                    }
                                    // Отключение изолинии
                                    else
                                    {
                                        if (isoline.IsIsoline)
                                        {
                                            isoline.Activate(false);
                                        }
                                    }
                                }
                            }
                            t.Commit();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Превратить все штрихи в отдельные линии
        /// </summary>
        public static void FreezeAll()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            using (doc.LockDocument())
            {
                Database db = doc.Database;
                RegAppIsoline(db);
                using (var t = db.TransactionManager.StartTransaction())
                {
                    var bt = db.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable;
                    foreach (var idBtr in bt)
                    {
                        var btr = idBtr.GetObject(OpenMode.ForRead) as BlockTableRecord;

                        List<Line> linesInBtr = new List<Line>();
                        foreach (var idEnt in btr)
                        {
                            if (idEnt.ObjectClass.IsDerivedFrom(rxCurve))
                            {
                                var curve = idEnt.GetObject(OpenMode.ForRead, false, true) as Curve;
                                Isoline isoline = new Isoline(curve);
                                if (isoline.IsIsoline)
                                {
                                    var lines = isoline.GetLines(curve);
                                    linesInBtr.AddRange(lines);
                                }
                            }
                        }
                        if (linesInBtr.Count > 0)
                        {
                            btr.UpgradeOpen();
                            foreach (var line in linesInBtr)
                            {
                                // Пометить линию как бергштрих
                                SetDashXData(line);
                                btr.AppendEntity(line);
                                t.AddNewlyCreatedDBObject(line, true);
                            }
                        }
                    }
                    t.Commit();
                }
            }
        }

        public static void RegAppIsoline(Database db)
        {
            using (RegAppTable rat = db.RegAppTableId.Open(OpenMode.ForRead, false) as RegAppTable)
            {
                if (!rat.Has(RegAppNAME))
                {
                    rat.UpgradeOpen();
                    using (RegAppTableRecord ratr = new RegAppTableRecord())
                    {
                        ratr.Name = RegAppNAME;
                        rat.Add(ratr);
                    }
                }
            }
        }

        public static void RemoveXData(DBObject dbo)
        {
            if (dbo.GetXDataForApplication(RegAppNAME) != null)
            {
                ResultBuffer rb = rb = new ResultBuffer(new TypedValue(1001, RegAppNAME));
                dbo.UpgradeOpen();
                dbo.XData = rb;
                dbo.DowngradeOpen();
            }
        }

        public static void UnfreezeAll()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            using (doc.LockDocument())
            {
                Database db = doc.Database;
                RegAppIsoline(db);
                using (var t = db.TransactionManager.StartTransaction())
                {
                    var bt = db.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable;
                    foreach (var idBtr in bt)
                    {
                        var btr = idBtr.GetObject(OpenMode.ForRead) as BlockTableRecord;
                        List<Line> linesInBtr = new List<Line>();
                        foreach (var idEnt in btr)
                        {
                            if (idEnt.ObjectClass.Name == "AcDbLine")
                            {
                                var line = idEnt.GetObject(OpenMode.ForRead, false, true) as Line;
                                Isoline isoline = new Isoline(line);
                                // Если это штрих, то удвляем ее, она автоматом построится при overrule
                                if (isoline.isDash)
                                {
                                    line.UpgradeOpen();
                                    line.Erase();
                                }
                            }
                        }
                    }
                    t.Commit();
                }
            }
        }

        /// <summary>
        /// Включение/отключение бергштрихов для полилинии - запись xdata
        /// Должна быть запущена транзакция!!!
        /// </summary>
        private void Activate(bool activate)
        {
            using (var curve = idCurve.GetObject(OpenMode.ForRead, false, true) as Curve)
            {
                //ResultBuffer rb = curve.GetXDataForApplication(RegAppNAME);
                ResultBuffer rb;
                if (activate)
                {
                    rb = new ResultBuffer(new TypedValue(1001, RegAppNAME),
                                              // 0 - прямой штрих, 1 - обратный
                                              new TypedValue((int)DxfCode.ExtendedDataInteger16, 0),
                                              // 0 - изолиния, 1 - бергштрих
                                              new TypedValue((int)DxfCode.ExtendedDataInteger16, 0)
                                          );
                }
                else
                {
                    rb = new ResultBuffer(new TypedValue(1001, RegAppNAME));
                }
                curve.UpgradeOpen();
                curve.XData = rb;
            }
        }

        public List<Line> GetLines(Curve curve)
        {
            List<Line> lines = new List<Line>();
            if (curve is Polyline)
            {
                Polyline pl = (Polyline)curve;
                for (int i = 0; i < pl.NumberOfVertices; i++)
                {
                    SegmentType segmentType = pl.GetSegmentType(i);
                    if (segmentType == SegmentType.Line)
                    {
                        var lineSegment = pl.GetLineSegmentAt(i);
                        var line = getLine(lineSegment, curve);
                        lines.Add(line);
                    }
                }
            }
            else if (curve is Line)
            {
                Line lineCurve = (Line)curve;
                LineSegment3d segment = new LineSegment3d(lineCurve.StartPoint, lineCurve.EndPoint);
                var line = getLine(segment, curve);
                lines.Add(line);
            }
            return lines;
        }

        /// <summary>
        /// обратить бергштрихи у полилинии
        /// Должна быть запущена транзакция!!!
        /// </summary>
        public void ReverseIsoline()
        {
            using (var curve = idCurve.GetObject(OpenMode.ForWrite, false, true) as Curve)
            {
                ResultBuffer rb = curve.GetXDataForApplication(RegAppNAME);
                if (rb != null)
                {
                    var data = rb.AsArray();
                    bool isFound = false;
                    for (int i = 0; i < data.Length; i++)
                    {
                        var tv = data[i];
                        if (tv.TypeCode == (short)DxfCode.ExtendedDataInteger16)
                        {
                            data[i] = new TypedValue((int)DxfCode.ExtendedDataInteger16, isNegate ? 0 : 1);
                            isNegate = !isNegate;
                            isFound = true;
                            break;
                        }
                    }
                    if (isFound)
                    {
                        using (ResultBuffer rbNew = new ResultBuffer(data))
                        {
                            curve.XData = rbNew;
                        }
                    }
                }
            }
        }

        public static void ReverseIsolines()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            using (doc.LockDocument())
            {
                Editor ed = doc.Editor;
                RegAppIsoline(doc.Database);
                PromptSelectionResult result = ed.SelectImplied();
                if (result.Status == PromptStatus.OK)
                {
                    var selIds = result.Value.GetObjectIds();
                    if (selIds.Count() > 0)
                    {
                        using (var t = doc.Database.TransactionManager.StartTransaction())
                        {
                            foreach (var item in selIds)
                            {
                                if (item.ObjectClass.IsDerivedFrom(Isoline.rxCurve))
                                {
                                    Isoline isoline = new Isoline(item);
                                    if (isoline.IsIsoline)
                                    {
                                        isoline.ReverseIsoline();
                                    }
                                }
                            }
                            t.Commit();
                        }
                    }
                }
            }
        }

        private static void SetDashXData(Line line)
        {
            using (ResultBuffer rb = new ResultBuffer(new TypedValue(1001, RegAppNAME),
                                         new TypedValue((int)DxfCode.ExtendedDataInteger16, 0),
                                         new TypedValue((int)DxfCode.ExtendedDataInteger16, 1)))
            {
                line.XData = rb;
            }
        }

        private void getIsolineProperties(Curve curve)
        {
            ResultBuffer rb = curve.GetXDataForApplication(RegAppNAME);
            if (rb != null)
            {
                TypedValue[] tvs = rb.AsArray();
                int countTvs = tvs.Count();
                if (countTvs > 1)
                {
                    TypedValue tvNegate = tvs[1];
                    if (tvNegate.TypeCode == (short)DxfCode.ExtendedDataInteger16)
                    {
                        isNegate = Convert.ToBoolean(tvNegate.Value);
                    }
                }
                if (countTvs > 2)
                {
                    TypedValue tvTypeIsoline = tvs[2];
                    if (tvTypeIsoline.TypeCode == (short)DxfCode.ExtendedDataInteger16)
                    {
                        isDash = Convert.ToBoolean(tvTypeIsoline.Value);
                    }
                }
                IsIsoline = !isDash;
            }
        }

        private Line getLine(LineSegment3d segment, Curve curve)
        {
            Vector3d vectorIsoline = segment.Direction.GetPerpendicularVector() * _options.DashLength;
            if (isNegate)
            {
                vectorIsoline = vectorIsoline.Negate();
            }
            Point3d ptEndIsoline = segment.MidPoint + vectorIsoline;
            Line line = new Line(segment.MidPoint, ptEndIsoline);
            line.Layer = curve.Layer;
            line.LineWeight = curve.LineWeight;
            line.Color = curve.Color;
            line.Linetype = SymbolUtilityServices.LinetypeContinuousName;
            return line;
        }
    }
}