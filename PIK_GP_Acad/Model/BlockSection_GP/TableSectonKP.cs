using System;
using System.Linq;
using AcadLib.Jigs;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace PIK_GP_Acad.BlockSection_GP
{
    // Построение таблицы блок-секций
    public class TableSectonKP
    {
        private SectionService _service;

        public TableSectonKP(SectionService sectionService)
        {
            _service = sectionService;
        }

        public void CreateTable()
        {
            Database db = _service.Doc.Database;
            Table table = getTable(db);
            InsertTable(table);
        }

        private Table getTable(Database db)
        {
            Table table = new Table();
            table.SetDatabaseDefaults(db);
            table.TableStyle = db.GetTableStylePIK(); // если нет стиля ПИк в этом чертеже, то он скопируетс из шаблона, если он найдется

            var data = _service.DataSection;

            table.SetSize(5, 3);

            table.Columns[0].Width = 40;
            table.Columns[1].Width = 40;
            table.Columns[2].Width = 40;

            table.SetRowHeight(8);

            var lwBold = LineWeight.LineWeight030;

            // Фон таблицы в зависимости от региона
            _service.Estimate.TableFormatting(table);
            
            table.Columns[0].Alignment = CellAlignment.MiddleLeft;
            table.Columns[1].Alignment = CellAlignment.MiddleCenter;
            table.Columns[2].Alignment = CellAlignment.MiddleCenter;
            //table.Rows[1].Height = 15;

            table.Cells[0, 0].TextString = _service.Estimate.Title;
            table.Cells[0, 0].Alignment = CellAlignment.MiddleCenter;
            
            table.Cells[1, 1].TextString = "Площадь в г.н.с.";
            table.Cells[1, 2].TextString = "Жилой фонд";
            //table.Cells[1, 0].Borders.Bottom.LineWeight = LineWeight.LineWeight030;            
            
            var titleCells = CellRange.Create(table, 1, 0, 1, table.Columns.Count - 1);
            titleCells.Borders.Bottom.LineWeight = lwBold;

            table.Cells[2, 0].TextString = "1 этаж БКФН";
            table.Cells[2, 1].TextString = data.KP_GNS_BKFN.ToString("0.0");
            table.Cells[2, 2].TextString = data.TotalAreaBKFN.ToString("0.0");
            //table.Cells[1, 1].Borders.Bottom.LineWeight = LineWeight.LineWeight030;

            table.Cells[3, 0].TextString = "Типовой этаж квартиры";
            table.Cells[3, 1].TextString = data.KP_GNS_Typical.ToString("0.0");
            table.Cells[3, 2].TextString = data.TotalAreaApart.ToString("0.0");

            table.Cells[4, 0].TextString = "ИТОГО";
            table.Cells[4, 1].TextString = data.KP_GNS_Total.ToString("0.0");
            table.Cells[4, 2].TextString = data.TotalArea.ToString("0.0");

            var lastRow = table.Rows.Last();
            lastRow.Borders.Bottom.LineWeight = lwBold;

            table.GenerateLayout();
            return table;
        }

        // вставка
        private void InsertTable(Table table)
        {
            Database db = _service.Doc.Database;
            Editor ed = _service.Doc.Editor;

            TableJig jigTable = new TableJig(table, 1 / db.Cannoscale.Scale, "Вставка таблицы блок-секций");
            if (ed.Drag(jigTable).Status == PromptStatus.OK)
            {
                using (var t = db.TransactionManager.StartTransaction())
                {
                    //table.ScaleFactors = new Scale3d(100);
                    var cs = db.CurrentSpaceId.GetObject(OpenMode.ForWrite) as BlockTableRecord;
                    cs.AppendEntity(table);
                    t.AddNewlyCreatedDBObject(table, true);
                    t.Commit();
                }
            }
        }
    }
}