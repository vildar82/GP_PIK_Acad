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
        public DateTime Date { get; set; }

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
                        worksheet.Cells[cell.Row, cell.Column].Value = cell.InsValue.ToString();
                    }
                }
                xlPackage.Save();
            }
        }      
        
        /// <summary>
        /// Сохранение данных инсоляции в базу
        /// </summary>
        public void ToDb()
        {
            var insData = new PIK_DB_Projects.Ins.InsData();
            insData.Date = Date;
            insData.Name = FrontGroup.Name;
            insData.Objects = Houses.Select(s => new PIK_DB_Projects.Ins.InsObject
            {
                Id = s.Id,
                Points = s.Cells.Select(p => new PIK_DB_Projects.Ins.InsPoint
                {
                    Row = p.Row,
                    Column = p.Column,
                    InsValue = p.GetInsValue()
                }).ToList()
            }).ToList();

            PIK_DB_Projects.Ins.DbInsService.Save(insData);
        }  
    }
}
