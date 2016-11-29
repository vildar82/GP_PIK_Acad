using AcadLib.Tables;
using PIK_GP_Acad.Insolation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.Insolation.Services
{
    /// <summary>
    /// Отчет по точке инсоляции
    /// </summary>
    public class InsPointReport : CreateTable
    {
        private IInsPoint insPoint;
        public InsPointReport (IInsPoint insPoint, Database db) : base(db)
        {
            this.insPoint = insPoint;
            Title = "Расчетная точка №" + insPoint?.Number;
        }

        public override void CalcRows()
        {
            NumColumns = 2;
            var ilums = insPoint?.Illums;
            if (ilums == null || ilums.Count == 0) return;

            NumRows = ilums.Count +2+2; // 2- итоговые строчки
        }

        protected override void FillCells(Table table)
        {
            var ilums = insPoint?.Illums;
            if (ilums == null || ilums.Count == 0) return;

            int row = 2;
            Cell cell;
            foreach (var item in ilums)
            {
                cell = table.Cells[row, 0];
                cell.TextString = item.TimeStart;
                cell = table.Cells[row, 1];
                cell.TextString = item.TimeEnd;
                row++;
            }
            // Строки итогов
            // Макс Непрерывная инс           
            table.MergeCells(table.Rows[row]);
            cell = table.Cells[row, 0];
            cell.TextString = $"Продолжительность непрерывной инсоляции {insPoint.InsValue.MaxContinuosTimeString}";
            row++;
            // итого прерыв инс
            table.MergeCells(table.Rows[row]);
            cell = table.Cells[row, 0];
            cell.TextString = $"Сумма прерывистой инсоляции {insPoint.InsValue.TotalTimeString}";
        }

        protected override void SetColumnsAndCap(ColumnsCollection columns)
        {
            var col = columns[0];
            col.Alignment = CellAlignment.MiddleCenter;
            col.Width = 30;
            col[1, 0].TextString = "Начало";
            col = columns[1];
            col.Alignment = CellAlignment.MiddleCenter;
            col.Width = 30;
            col[1, 1].TextString = "Конец";
        }
    }
}
