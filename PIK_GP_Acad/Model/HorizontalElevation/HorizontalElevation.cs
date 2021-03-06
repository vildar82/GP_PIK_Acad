﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Runtime;
using AcadLib;

namespace PIK_GP_Acad.HorizontalElevation
{
    /// <summary>
    /// Изменение уровней горизонталей
    /// </summary>
    public class HorizontalElevationService
    {        
        Database db;
        Editor ed;
        TransientManager tm;
        List<DBText> tempTexts;
        double curElev;
        double stepElev;
        Color color;

        public HorizontalElevationService()
        {            
        }

        /// <summary>
        /// Поочередное изменени уровня выбранной горизонтали на заданный шаг от стартового значения.
        /// </summary>
        public void Stepping()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            db = doc.Database;
            ed = doc.Editor;

            loadStartLevels();
            var formHorElev = new FormHorizontalElevation(curElev, stepElev, color.ColorValue);
            if (Application.ShowModalDialog(formHorElev) == System.Windows.Forms.DialogResult.OK)
            {
                curElev = formHorElev.StartElevation;
                stepElev = formHorElev.StepElevation;
                color = Color.FromColor(formHorElev.Color);

                using (var t = db.TransactionManager.StartTransaction())
                {
                    tempTexts = new List<DBText>();
                    tm = TransientManager.CurrentTransientManager;
                    bool isContinue = true;
                    do
                    {
                        Point3d ptPicked;
                        var plId = getHorizontal(curElev, out ptPicked);
                        if (plId.IsNull)
                        {
                            ed.WriteMessage("\nПрервано пользоваателем.");
                            isContinue = false;
                        }
                        else
                        {
                            var pl = plId.GetObject(OpenMode.ForWrite, false, true) as Autodesk.AutoCAD.DatabaseServices.Polyline;
                            if (pl == null)
                            {
                                ed.WriteMessage("\nПрервано - Не удалось определить выбранный объект.");
                                isContinue = false;
                            }
                            else
                            {
                                pl.Elevation = curElev;
                                pl.Color = color;
                                // Текст назначенного уровня для полилинии
                                addText(curElev, ptPicked);
                                // Изменение текущего уровня на шаг
                                curElev += stepElev;
                            }
                        }
                    } while (isContinue);
                    saveStartLevels();
                    ClearTransientGraphics();
                    t.Commit();
                }
            }
        }

        public static System.Drawing.Color SelectColor(System.Drawing.Color color)
        {
            var colorDialog = new Autodesk.AutoCAD.Windows.ColorDialog();
            colorDialog.Color = Color.FromColor(color);
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return colorDialog.Color.ColorValue;
            }
            return color;
        }

        private void saveStartLevels()
        {
            try
            {
                DictNOD nod = new DictNOD("GP-HosizontalElevations");
                nod.Save(curElev, "StartElevation");
                nod.Save(stepElev, "StepElevation");
                using (var reg = new AcadLib.Registry.RegExt("HosizontalElevations"))
                {
                    reg.Save("Color", GetColorString(color));
                }
            }
            catch (System.Exception ex)
            {
                Logger.Log.Error(ex, "HorizontalElevationService.saveStartLevels()");
            }
        }       

        private void loadStartLevels()
        {
            try
            {
                DictNOD nod = new DictNOD("GP-HosizontalElevations");
                curElev = nod.Load("StartElevation", HorizontalElevationOptions.Instance.StartElevation);
                stepElev = nod.Load("StepElevation", HorizontalElevationOptions.Instance.StepElevation);
                using (var reg = new AcadLib.Registry.RegExt("HosizontalElevations"))
                {
                    var colorString = reg.Load("Color", "");
                    try
                    {
                        color = GetColor(colorString);
                    }
                    catch (System.Exception ex)
                    {
                        Logger.Log.Error(ex, "HorizontalElevationService.loadStartLevels()");
                        color = Color.FromColorIndex(ColorMethod.ByAci, 3);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Logger.Log.Error(ex, "HorizontalElevationService.loadStartLevels()");
            }
        }        

        private void addText(double level, Point3d pt)
        {
            DBText text = new DBText();
            text.SetDatabaseDefaults();
            text.TextString = level.ToString();
            text.Height = ed.GetCurrentView().Height * HorizontalElevationOptions.Instance.TextHeight;
            text.ColorIndex = 11;//Color.FromColor(HorizontalElevationOptions.Instance.TextColor);
            text.Position = pt;
            text.Justify = AttachmentPoint.MiddleCenter;
            text.AlignmentPoint = pt;
            text.AdjustAlignment(db);

            tm.AddTransient(text, TransientDrawingMode.Main, 0, new IntegerCollection());
            tempTexts.Add(text);
        }

        private ObjectId getHorizontal(double curElev, out Point3d ptPicked)
        {
            var prOpt = new PromptEntityOptions($"\nВыберите горизонталь для установки ей уровня {curElev}:");
            prOpt.SetRejectMessage("\nМожно выбрать только полилинию");
            prOpt.AddAllowedClass(typeof(Autodesk.AutoCAD.DatabaseServices.Polyline), true);
            prOpt.AllowNone = false;
            prOpt.AllowObjectOnLockedLayer = true;

            var prRes = ed.GetEntity(prOpt);
            if (prRes.Status == PromptStatus.OK)
            {
                ptPicked = prRes.PickedPoint;
                return prRes.ObjectId;
            }
            ptPicked = Point3d.Origin;
            return ObjectId.Null;
        }

        // Erases any transient graphics
        private void ClearTransientGraphics()
        {
            IntegerCollection intCol = new IntegerCollection();
            if (tempTexts != null)
            {
                foreach (DBObject transient in tempTexts)
                {
                    tm.EraseTransient(
                                        transient,
                                        intCol
                                        );
                    transient.Dispose();
                }
                tempTexts.Clear();
            }
        }

        private Color GetColor(string colorString)
        {
            var s = colorString.Split(';');            
            byte r = byte.Parse(s[0]);
            byte g = byte.Parse(s[1]);
            byte b = byte.Parse(s[2]);
            return Color.FromRgb(r,g,b);
        }
        private string GetColorString(Color color)
        {
            return $"{color.Red};{color.Green};{color.Blue}";
        }
    }
}
