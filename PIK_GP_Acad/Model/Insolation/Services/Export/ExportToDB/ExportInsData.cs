using OfficeOpenXml;
using PIK_GP_Acad.Insolation.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Acad.Insolation.Services.Export
{
    /// <summary>
    /// Данные экпорта - одного расчета жуков (ячейки инс не более 4 домов)
    /// </summary>
    public class ExportInsData
    {
        public ExportInsData(FrontGroup front, List<HouseTransform> houses)
        {
            Houses = houses;
            FrontGroup = front;
        }

        public List<HouseTransform> Houses { get; set; }
        public FrontGroup FrontGroup { get; set; }

        /// <summary>
        /// Запись данных в Excel (для проверки)
        /// </summary>
        public void ToExel(string fileOutput)
        {
            if (Houses == null) return;

            if (File.Exists(fileOutput))
            {
                File.Delete(fileOutput);
            }

            using (var xlPackage = new ExcelPackage(new FileInfo(fileOutput)))
            {
                var worksheet = xlPackage.Workbook.Worksheets.Add("Trest");
                foreach (var house in Houses)
                {
                    foreach (var cell in house.Cells)
                    {
                        worksheet.Cells[cell.Row+1, cell.Column+1].Value = cell.InsValue.ToString();
                    }
                }
                xlPackage.Save();
            }
        }        
    }
}
