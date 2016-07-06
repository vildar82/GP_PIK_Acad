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
        private DataSection data;

        public TableSection(DataSection dataSec)
        {
            this.data = dataSec;
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

            table.SetSize(5, 9);
            table.SetBorders(LineWeight.LineWeight050);
            table.SetRowHeight(8);

            var rowHeaders = table.Rows[1];
            rowHeaders.Height = 15;
            var lwBold = rowHeaders.Borders.Top.LineWeight;
            rowHeaders.Borders.Bottom.LineWeight = lwBold;

            var titleCells = table.Cells[0, 0];
            titleCells.TextString = DateTime.Now.ToString();
            titleCells.Alignment = CellAlignment.MiddleCenter;

            var col = table.Columns[0];
            col[2, 0].TextString = "1 этаж";
            col[3, 0].TextString = "Типовые этажи";
            col[4, 0].TextString = "Итого";
            col.Alignment = CellAlignment.MiddleCenter;
            col.Width = 30;

            // Площадь в г.н.с.
            col = table.Columns[1];
            col[1, 1].TextString = "Площадь в Г.Н.С.,\nтыс. м" + General.Symbols.Square;
            col[2, 1].TextString = (data.AreaFirstExternalWalls*0.001).ToString("0.00"); //"1 этаж"
            col[3, 1].TextString = (data.AreaUpperExternalWalls*0.001).ToString("0.00"); //"Верхние этажи"
            col[4, 1].TextString = (data.AreaTotalExternalWalls*0.001).ToString("0.00"); //"Итого"
            col.Alignment = CellAlignment.MiddleCenter;
            col.Width = 20;

            // Площадь ж.ф.
            col = table.Columns[2];
            col[1, 2].TextString = "Площадь Ж.Ф.,\nтыс. м" + General.Symbols.Square;
            col[2, 2].TextString = (data.AreaFirstLive*0.001).ToString("0.00"); //"1 этаж"
            col[3, 2].TextString = (data.AreaUpperLive*0.001).ToString("0.00"); //"Верхние этажи"
            col[4, 2].TextString = (data.AreaTotalLive*0.001).ToString("0.00"); //"Итого"
            col.Alignment = CellAlignment.MiddleCenter;
            col.Width = 20;
            
            SetNorm("Население, чел", data.Population.ToString(), table, 3);
            SetNorm("СОШ, мест", data.SchoolPlaces.ToString(), table, 4);
            SetNorm("ДОО, мест", data.KinderPlaces.ToString(), table, 5);
            SetNorm("Постоянный паркинг, м/м", data.PersistentParking.ToString(), table, 6);
            SetNorm("Временный паркинг, м/м", data.TemproraryParking.ToString(), table, 7);
            SetNorm("Паркинг для БКФН, м/м", data.ParkingBKFN.ToString(), table, 8);

            var lastRow = table.Rows.Last();
            lastRow.Borders.Bottom.LineWeight = lwBold;

            table.GenerateLayout();
            return table;
        }

        private void SetNorm (string title, string value, Table table, int colIndex)
        {
            var col = table.Columns[colIndex];
            col[1, colIndex].TextString = title;
            var mCells = CellRange.Create(table, 2, colIndex, 4, colIndex);
            table.MergeCells(mCells);
            col[2, colIndex].TextString = value;
            col.Alignment = CellAlignment.MiddleCenter;
            col.Width = 20;
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
