﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AcadLib.Errors;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using AcadLib.Layers;

namespace PIK_GP_Acad.Parking
{
    /// <summary>
    /// Линейные парковки
    /// </summary>
    public class LineParkingService
    {
        public Document Doc { get; set; }
        public Database Db { get; set; }
        public Editor Ed { get; set; }
        public LineParkingOptions Options { get; set; }
        public LineParkingData Data { get; set; }
        public bool IsTableOrText { get; set; }

        /// <summary>
        /// Расчет машиномест выбранных блоков линейных парковок
        /// </summary>
        public void CalcAndTable()
        {
            Doc = Application.DocumentManager.MdiActiveDocument;
            Db = Doc.Database;
            Ed = Doc.Editor;
            Options = LineParkingOptions.Load();

            var parkings = Select();
            if (parkings.Count==0)
            {
                Ed.WriteMessage($"\nБлоки линейных парковок не найдены.");
                return;
            }

            // Расчет машиномест для каждой парковки
            foreach (var parking in parkings)            
                parking.Calc();            

            // Суммирование парковок
            Data = new LineParkingData(parkings);
            Data.Calc();

            if (Data.Places ==0 && Data.InvalidPlaces==0)
            {
                Ed.WriteMessage("\nНет парковочных мест");
                return;
            }

            // Вставка таблицы
            if (IsTableOrText)
            {
                LineParkingTable table = new LineParkingTable(this);
                table.CreateTable();
            }
            else
            {
                using (var t = Db.TransactionManager.StartTransaction())
                {
                    // Вставка текста
                    DBText text = new DBText();
                    text.SetDatabaseDefaults();
                    text.Height = 2.5 * AcadLib.Scale.ScaleHelper.GetCurrentAnnoScale(Db);                    

                    // стиль текста
                    text.TextStyleId = Db.GetTextStylePIK();

                    if (Data.Places != 0)
                    {
                        text.TextString = $"Мм={Data.Places}";
                    }
                    if (Data.InvalidPlaces != 0)
                    {
                        text.TextString += $"{(Data.Places == 0? "": ";")} Мм инв.={Data.InvalidPlaces}";
                    }
                    Point3d ptText = Point3d.Origin;
                    text.Position = ptText;
                    text.LayerId = ParkingHelper.LayerTextId;

                    var ms = Db.CurrentSpaceId.GetObject(OpenMode.ForWrite) as BlockTableRecord;
                    ms.AppendEntity(text);
                    t.AddNewlyCreatedDBObject(text, true);

                    PromptSelectionResult prs = Ed.SelectLast();                    
                    if (prs.Status == PromptStatus.OK)
                    {
                        PromptPointResult ppr = Ed.Drag(prs.Value, "\nТочка вставки текста:", (Point3d pt, ref Matrix3d mat) =>
                        {
                            if (ptText == pt) return SamplerStatus.NoChange;
                            mat = Matrix3d.Displacement(ptText.GetVectorTo(pt));
                            text.TransformBy(mat);
                            ptText = pt;
                            return SamplerStatus.OK;
                        });    
                        if (ppr.Status != PromptStatus.OK)
                        {
                            text.Erase();
                        }
                    }                    
                    t.Commit();
                }                
            }
        }

        private List<LineParking> Select()
        {
            List<LineParking> parkings = new List<LineParking>();

            IEnumerable ids = null;
            try
            {
                ids = SelectPrompt();
            }
            catch (ArgumentNullException)
            {                
            }            

            using (var t = Db.TransactionManager.StartTransaction())
            {
                if (ids == null)
                {
                    IsTableOrText = true;
                    var ms = Db.CurrentSpaceId.GetObject(OpenMode.ForRead) as BlockTableRecord;
                    ids = ms;
                }

                int countBlParking = 0;
                foreach (ObjectId idEnt in ids)
                {
                    var blRef = idEnt.GetObject(OpenMode.ForRead, false, true) as BlockReference;
                    if (blRef == null) continue;

                    var blName = blRef.GetEffectiveName();
                    if (IsBlockLineParking(blName))
                    {
                        LineParking lp = new LineParking();
                        var res = lp.Define(blRef);
                        if (res.Failure)
                        {
                            Inspector.AddError("Пропущен блок парковки с ошибками:" + res.Error,
                                blRef, System.Drawing.SystemIcons.Error);
                        }
                        else
                        {
                            countBlParking++;
                            parkings.Add(lp);
                        }
                    }
                }
                t.Commit();
                Doc.Editor.WriteMessage($"\nВыбрано блоков линейной парковки: {countBlParking}");
            }
            return parkings;
        }

        private IEnumerable SelectPrompt()
        {
            IEnumerable ids = null;
            var selOpt = new PromptSelectionOptions();
            selOpt.Keywords.Add("Table");
            var keys = selOpt.Keywords.GetDisplayString(true);
            selOpt.MessageForAdding = "\nВыбор блоков линейных парковок: " + keys;
            selOpt.MessageForRemoval = "\nИсключение блоков: " + keys;

            selOpt.KeywordInput += (o, e) =>
            {
                throw new ArgumentNullException();
            };

            var sel = Ed.GetSelection(selOpt);
            if (sel.Status == PromptStatus.OK)
            {
                ids = sel.Value.GetObjectIds();
            }
            else if (sel.Status != PromptStatus.Keyword)
            {
                throw new Exception(AcadLib.General.CanceledByUser);
            }
            return ids;
        }

        private bool IsBlockLineParking(string blName)
        {
            return Regex.IsMatch(blName, Options.BlockNameLineParkingMatch, 
                RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);            
        }
    }
}
