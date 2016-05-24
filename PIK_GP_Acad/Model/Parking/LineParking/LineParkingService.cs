using System;
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

namespace PIK_GP_Acad.Parking
{
    /// <summary>
    /// Линейные парковки
    /// </summary>
    public static class LineParkingService
    {
        public static Document Doc { get; set; }
        public static Database Db { get; set; }
        public static LineParkingOptions Options { get; set; }

        /// <summary>
        /// Расчет машиномест выбранных блоков линейных парковок
        /// </summary>
        public static void CalcAndTable()
        {
            Doc = Application.DocumentManager.MdiActiveDocument;
            Db = Doc.Database;
            Options = LineParkingOptions.Load();

            var parkings = Select();
            if (parkings.Count==0)
            {
                Doc.Editor.WriteMessage($"\nБлоки линейных парковок не найдены.");
                return;
            }

            // Расчет машиномест для каждой парковки
            foreach (var parking in parkings)            
                parking.Calc();            

            // Суммирование парковок
            LineParkingData data = new LineParkingData(parkings);
            data.Calc();

            // Вставка таблицы
            LineParkingTable table = new LineParkingTable();
            table.CreateTable(data);
        }

        private static List<LineParking> Select(bool InAllModel = false)
        {
            List<LineParking> parkings = new List<LineParking>();

            using (var t = Db.TransactionManager.StartTransaction())
            {
                IEnumerable ids;
                if (InAllModel)
                {
                    var ms = SymbolUtilityServices.GetBlockModelSpaceId(Db).GetObject(OpenMode.ForRead) as BlockTableRecord;
                    ids = ms;
                }
                else
                {
                    Editor ed = Doc.Editor;
                    var sel = ed.SelectBlRefs("\nВыбор блоков линейных парковок:");
                    ids = sel;
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

        private static bool IsBlockLineParking(string blName)
        {
            return Regex.IsMatch(blName, Options.BlockNameLineParkingMatch, 
                RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);            
        }
    }
}
