using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Jigs;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using AcadLib;

namespace PIK_GP_Acad.KP.KP_BlockSection
{
    class TableSection
    {
        private DataSection dataSec;

        public TableSection(DataSection dataSec)
        {
            this.dataSec = dataSec;
        }

        internal void Create()
        {
            Table table = getTable();
            InsertTable(table);
        }        

        private Table getTable()
        {
            Table table = new Table();
            table.SetDatabaseDefaults();
            table.TableStyle = KP_BlockSectionService.Db.GetTableStylePIK();

            table.SetSize(5,3);
            table.SetBorders(LineWeight.LineWeight050);
            table.SetRowHeight(8);

            var rowHeaders = table.Rows[1];
            rowHeaders.Height = 15;
            var lwBold = rowHeaders.Borders.Top.LineWeight;
            rowHeaders.Borders.Bottom.LineWeight = lwBold;

            var col = table.Columns[0];
            col.Alignment = CellAlignment.MiddleLeft;
            col.Width = 30;

            col = table.Columns[1];
            col.Alignment = CellAlignment.MiddleCenter;
            col.Width = 30;

            col = table.Columns[2];
            col.Alignment = CellAlignment.MiddleCenter;
            col.Width = 30;

            var titleCells = table.Cells[0, 0];
            titleCells.TextString = DateTime.Now.ToString();
            titleCells.Alignment = CellAlignment.MiddleCenter;

            table.Cells[1, 1].TextString = "Площадь в г.н.с.";
            table.Cells[1, 2].TextString = "Площадь ж.ф.";

            table.Cells[2, 0].TextString = "1 этаж";
            table.Cells[3, 0].TextString = "Типовые этажи";
            table.Cells[4, 0].TextString = "Итого";
            
            table.Cells[2, 1].TextString = dataSec.AreaFirstExternalWalls.ToString("0.00"); //"1 этаж"
            table.Cells[3, 1].TextString = dataSec.AreaUpperExternalWalls.ToString("0.00"); //"Верхние этажи"
            table.Cells[4, 1].TextString = dataSec.AreaTotalExternalWalls.ToString("0.00"); //"Итого"

            table.Cells[2, 2].TextString = dataSec.AreaFirstLive.ToString("0.00"); //"1 этаж"
            table.Cells[3, 2].TextString = dataSec.AreaUpperLive.ToString("0.00"); //"Верхние этажи"
            table.Cells[4, 2].TextString = dataSec.AreaTotalLive.ToString("0.00"); //"Итого"

            var lastRow = table.Rows.Last();
            lastRow.Borders.Bottom.LineWeight = lwBold;

            table.GenerateLayout();
            return table;
        }

        private void InsertTable(Table table)
        {
            TableJig jigTable = new TableJig(table, 1 / KP_BlockSectionService.Db.Cannoscale.Scale, "Вставка таблицы блок-секций");
            if (KP_BlockSectionService.Ed.Drag(jigTable).Status == PromptStatus.OK)
            {
                using (var t = KP_BlockSectionService.Db.TransactionManager.StartTransaction())
                {
                    //table.ScaleFactors = new Scale3d(100);
                    var cs = KP_BlockSectionService.Db.CurrentSpaceId.GetObject(OpenMode.ForWrite) as BlockTableRecord;
                    cs.AppendEntity(table);
                    t.AddNewlyCreatedDBObject(table, true);
                    t.Commit();
                }
            }
        }
    }
}
