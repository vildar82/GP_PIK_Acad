using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Errors;
using AcadLib.Tables;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.Elements;
using PIK_GP_Acad.FCS;

namespace PIK_GP_Acad.FCS.Balance
{
    class BalanceTableService : CreateTable, ITableService
    {
        List<BalanceRow> data;
        List<IGrouping<ClassGroup, BalanceRow>> groupsData;
        private Color colorHomeRow = Color.FromRgb(199,200,202);
        private Color colorTotalRow = Color.FromRgb(199, 200, 202);

        public BalanceTableService (Database db) : base(db)
        {
        }       

        public void CalcRows (List<IGrouping<string, IArea>> groups)
        {
            data = new List<BalanceRow>();
            foreach (var item in groups)
            {
                BalanceRow row = new BalanceRow(item.ToList());
                data.Add(row);
            }

            // Строка Площади участка дома
            var homeRow = data.Find(r => r.ClassType.ClassName == BalanceClassService.HomeArea);
            if (homeRow != null)
            {
                // Процент остальных площадей
                foreach (var item in data)
                {
                    item.PercentTerritory = Math.Round(item.Area / homeRow.Area * 100,2);
                }
            }
            else
            {
                Inspector.AddError($"Не определен объект класса '{BalanceClassService.HomeArea}'");
            }

            CalcRows();
        }

        public override void CalcRows ()
        {
            Title = "Баланс территории жилого дома";
            NumColumns = 4;
            NumRows = data.Count+2;
            groupsData = data.GroupBy(g => g.ClassType.Group).ToList();
            var countGroups = groupsData.Where(g => g.Key != null).Count();
            NumRows += countGroups * 2; // Подзаголовок группы и итого
        }

        protected override void SetColumnsAndCap (ColumnsCollection columns)
        {
            int colIndex = 0;
            var col = columns[colIndex];
            col.Alignment = CellAlignment.MiddleCenter;
            col.Width = 10;
            var cell = col[1, colIndex];
            cell.TextString = "№ п/п";

            colIndex++;

            col = columns[colIndex];
            col.Alignment = CellAlignment.MiddleLeft;
            col.Width = 125;
            col.Borders.Horizontal.Margin = 2;
            cell = col[1, colIndex];
            cell.TextString = "Наименование";
            cell.Alignment = CellAlignment.MiddleCenter;

            colIndex++;

            col = columns[colIndex];
            col.Alignment = CellAlignment.MiddleCenter;
            col.Width = 25;
            cell = col[1, colIndex];
            cell.TextString = "Площадь, м" + AcadLib.General.Symbols.Square;            

            colIndex++;

            col = columns[colIndex];
            col.Alignment = CellAlignment.MiddleCenter;
            col.Width = 25;
            cell = col[1, colIndex];
            cell.TextString = "Процент территории, %";            
        }

        protected override void FillCells (Table table)
        {
            table.Cells.TextStyleId = db.GetTextStylePIK(PIK_GP_Acad.GPHelper.TextStylePikKP);

            int row = 2;
            Cell cell;
            CellRange mCells;
            int count =1;

            foreach (var itemGroup in groupsData)
            {
                if (itemGroup.Key != null)
                {
                    mCells = CellRange.Create(table, row, 0, row, table.Columns.Count - 1);
                    table.MergeCells(mCells);

                    cell = table.Cells[row, 0];
                    cell.TextString = itemGroup.Key.Name;
                    cell.Alignment = CellAlignment.MiddleCenter;
                    row++;
                }

                foreach (var itemRow in itemGroup)
                {
                    cell = table.Cells[row, 0];
                    cell.TextString = count++.ToString();

                    cell = table.Cells[row, 1];
                    cell.TextString = itemRow.Name;

                    cell = table.Cells[row, 2];
                    cell.TextString = itemRow.Area.ToString();

                    cell = table.Cells[row, 3];
                    cell.TextString = itemRow.PercentTerritory.ToString();                    

                    if (itemRow.ClassType.ClassName == BalanceClassService.HomeArea)
                    {
                        table.Rows[row].BackgroundColor = colorHomeRow;
                    }

                    row++; 
                }

                if (itemGroup.Key != null)
                {
                    mCells = CellRange.Create(table, row, 0, row, 1);
                    table.MergeCells(mCells);

                    cell = table.Cells[row, 0];
                    cell.TextString = itemGroup.Key.TotalName;
                    cell.Alignment = CellAlignment.MiddleRight;
                    cell.Borders.Horizontal.Margin = 2;

                    cell = table.Cells[row, 2];
                    cell.TextString = itemGroup.Sum(s => s.Area).ToString();

                    cell = table.Cells[row, 3];
                    cell.TextString = itemGroup.Sum(s => s.PercentTerritory).ToString();

                    table.Rows[row].BackgroundColor = colorTotalRow;

                    row++;
                }
            }
        }        
    }
}
