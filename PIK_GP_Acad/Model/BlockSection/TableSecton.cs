using System;
using AcadLib.Jigs;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace PIK_GP_Acad.BlockSection
{
    // Построение таблицы блок-секций
    public class TableSecton
    {
        private SectionService _service;

        public TableSecton(SectionService sectionService)
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

            table.SetSize(10, 2);

            table.Columns[0].Width = 55;
            table.Columns[1].Width = 25;

            foreach (var item in table.Rows)
            {
                item.Height = 8;
            }

            // Фон таблицы в зависимости от региона
            table.Cells.BackgroundColor = Color.FromColor(System.Drawing.Color.White);
            table.Color = _service.Estimate.Color;

            //foreach (var column in table.Columns)
            //{
            //    column.Width = 30;
            //    column.Alignment = CellAlignment.MiddleCenter;
            //}

            table.Columns[0].Alignment = CellAlignment.MiddleLeft;
            table.Columns[1].Alignment = CellAlignment.MiddleCenter;
            //table.Rows[1].Height = 15;

            table.Cells[0, 0].TextString = _service.Estimate.Title;;
            table.Cells[0, 0].Alignment = CellAlignment.MiddleCenter;

            //table.Cells[1, 0].TextString = "Наименование блок-секции";
            //table.Cells[1, 0].Alignment = CellAlignment.MiddleCenter;
            //table.Cells[2, 0].TextString = "Площадь квартир, м.кв.";
            //table.Cells[2, 0].Borders.Bottom.LineWeight = LineWeight.LineWeight030;
            //table.Cells[3, 0].TextString = "Площадь БКФН, м.кв.";
            //table.Cells[3, 0].Borders.Bottom.LineWeight = LineWeight.LineWeight030;
            //table.Cells[4, 0].TextString = "Количество секций";
            //table.Cells[4, 0].Borders.Bottom.LineWeight = LineWeight.LineWeight030;
            table.Cells[1, 0].TextString = "Всего площадь жилого фонда, м.кв.";
            table.Cells[1, 0].Borders.Bottom.LineWeight = LineWeight.LineWeight030;            
            table.Cells[2, 0].TextString = "Всего площадь квартир, м.кв.";
            table.Cells[2, 0].Borders.Bottom.LineWeight = LineWeight.LineWeight030;            
            table.Cells[3, 0].TextString = "Всего площадь БКФН, м.кв.";
            table.Cells[3, 0].Borders.Bottom.LineWeight = LineWeight.LineWeight030;            
            table.Cells[4, 0].TextString = "Средняя этажность";
            table.Cells[4, 0].Borders.Bottom.LineWeight = LineWeight.LineWeight030;            
            table.Cells[5, 0].TextString = $"Жителей, чел ({_service.Estimate.LiveAreaPerHuman}м2/чел)";
            table.Cells[5, 0].Borders.Bottom.LineWeight = LineWeight.LineWeight030;            
            table.Cells[6, 0].TextString = $"ДОО, чел ({_service.Estimate.KindergartenPlacePer1000}/1000)";
            table.Cells[6, 0].Borders.Bottom.LineWeight = LineWeight.LineWeight030;            
            table.Cells[7, 0].TextString = $"СОШ, чел ({_service.Estimate.SchoolPlacePer1000}/1000)";
            table.Cells[7, 0].Borders.Bottom.LineWeight = LineWeight.LineWeight030;            
            table.Cells[8, 0].TextString = $@"Машиноместа, м/м {_service.Estimate.ParkingPlacePer1000}/1000)х{_service.Estimate.ParkingPlacePercent}%)"; // "\\A1;\\pxt8;Машиноместа, м/м\\P\\ptz;{\\H0.6x;420/1 000}"
            table.Cells[8, 0].Borders.Bottom.LineWeight = LineWeight.LineWeight030;
            table.Cells[9, 0].TextString = $"Машиноместа гостевые, м/м ({_service.Estimate.ParkingPlaceGuestPercent}%)";
            table.Cells[9, 0].Borders.Bottom.LineWeight = LineWeight.LineWeight030;

            var titleCells = CellRange.Create(table, 1, 0, 1, table.Columns.Count - 1);
            titleCells.Borders.Bottom.LineWeight = LineWeight.LineWeight030;

            // Параметры по типам секций
            //int col = 1;
            //foreach (var sectType in data.SectionTypes)
            //{
            //    table.Cells[1, col].TextString = sectType.Name; //Наименование
            //    table.Cells[2, col].TextString = sectType.AreaApartTotal.ToString("0.0");  //Площадь квартир
            //    table.Cells[3, col].TextString = sectType.AreaBKFN.ToString("0.0");  // Площадь БКФН
            //    table.Cells[4, col].TextString = sectType.Count.ToString();// Кол секций
            //    col++;
            //}
            //var mCells = CellRange.Create(table, 5, 1, 5, table.Columns.Count - 1);
            //table.MergeCells(mCells);
            //mCells = CellRange.Create(table, 6, 1, 6, table.Columns.Count - 1);
            //table.MergeCells(mCells);
            //mCells = CellRange.Create(table, 7, 1, 7, table.Columns.Count - 1);
            //table.MergeCells(mCells);
            //mCells = CellRange.Create(table, 8, 1, 8, table.Columns.Count - 1);
            //table.MergeCells(mCells);
            //mCells = CellRange.Create(table, 9, 1, 9, table.Columns.Count - 1);
            //table.MergeCells(mCells);
            //mCells = CellRange.Create(table, 10, 1, 10, table.Columns.Count - 1);
            //table.MergeCells(mCells);
            //mCells = CellRange.Create(table, 11, 1, 11, table.Columns.Count - 1);
            //table.MergeCells(mCells);
            //mCells = CellRange.Create(table, 12, 1, 12, table.Columns.Count - 1);
            //table.MergeCells(mCells);

            // Общие параметры по всем типам секций

            // Всего площадь жилого фонда
            table.Cells[1, 1].TextString = (data.TotalArea).ToString("0.0");
            table.Cells[1, 1].Borders.Bottom.LineWeight = LineWeight.LineWeight030;
            // ВСЕГО ПЛОЩАДЬ КВАРТИР
            table.Cells[2, 1].TextString = data.TotalAreaApart.ToString("0.0");
            table.Cells[2, 1].Borders.Bottom.LineWeight = LineWeight.LineWeight030;
            // ВСЕГО ПЛОЩАДЬ БКФН
            table.Cells[3, 1].TextString = data.TotalAreaBKFN.ToString("0.0");
            table.Cells[3, 1].Borders.Bottom.LineWeight = LineWeight.LineWeight030;
            // Средняя этажность
            table.Cells[4, 1].TextString = data.AverageFloors.ToString("0.0"); 
            table.Cells[4, 1].Borders.Bottom.LineWeight = LineWeight.LineWeight030;
            // Жителей
            double population = Math.Floor(data.TotalAreaApart / _service.Estimate.LiveAreaPerHuman); // Всего площадь квартир/28
            table.Cells[5, 1].TextString = population.ToString();
            table.Cells[5, 1].Borders.Bottom.LineWeight = LineWeight.LineWeight030;
            //ДОО, чел
            table.Cells[6, 1].TextString =Math.Ceiling(population/1000*_service.Estimate.KindergartenPlacePer1000).ToString(); //(("Всего площадь квартир"/28)/1000)*65
            table.Cells[6, 1].Borders.Bottom.LineWeight = LineWeight.LineWeight030;
            //СОШ, чел
            table.Cells[7, 1].TextString = Math.Ceiling(population/1000*_service.Estimate.SchoolPlacePer1000).ToString();//  (("Всего площадь квартир"/28)/1000)*135
            table.Cells[7, 1].Borders.Bottom.LineWeight = LineWeight.LineWeight030;
            //Машиноместа, м/м
            var mm = population / 1000 * _service.Estimate.ParkingPlacePer1000;// 0.42;
            table.Cells[8, 1].TextString = Math.Ceiling(mm*_service.Estimate.ParkingPlacePercent/100).ToString();//  (("Всего площадь квартир"/28)/1000)*420 90%
            table.Cells[8, 1].Borders.Bottom.LineWeight = LineWeight.LineWeight030;
            //Машиноместа гостевые, м/м
            table.Cells[9, 1].TextString = Math.Ceiling(mm *_service.Estimate.ParkingPlaceGuestPercent/100).ToString();//  Машиноместа %25
            table.Cells[9, 1].Borders.Bottom.LineWeight = LineWeight.LineWeight030;

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