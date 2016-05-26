using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Jigs;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using AcadLib;

namespace PIK_GP_Acad.Parking
{
    public class LineParkingTable
    {
        LineParkingService service;
        LineParkingData data;
        Database db;

        public LineParkingTable(LineParkingService service)
        {
            this.service = service;
            db = service.Db;
            this.data = service.Data;
        }

        public void CreateTable()        {
            
            Table table = getTable();
            InsertTable(table);
        }

        private Table getTable()
        {
            Table table = new Table();
            table.SetDatabaseDefaults(db);
            table.TableStyle = db.GetTableStylePIK(); // если нет стиля ПИк в этом чертеже, то он скопируетс из шаблона, если он найдется            

            var boldLw = LineWeight.LineWeight040;

            table.SetSize(4, 2);
            table.SetBorders(boldLw);

            table.Columns[0].Width = 30;
            table.Columns[1].Width = 30;

            table.SetRowHeight(8);            

            table.Columns[0].Alignment = CellAlignment.MiddleLeft;
            table.Columns[1].Alignment = CellAlignment.MiddleCenter;            

            table.Cells[0, 0].TextString = "Линейные парковочные места";
            table.Cells[0, 0].Alignment = CellAlignment.MiddleCenter;            

            table.Cells[1, 0].TextString = "Наименование";
            table.Cells[1, 0].Alignment = CellAlignment.MiddleCenter;
            table.Cells[1, 1].TextString = "Кол-во";
            table.Rows[1].Height = 15;

            var cells = CellRange.Create(table, 1, 0, 1, table.Columns.Count - 1);
            cells.Borders.Bottom.LineWeight = boldLw;

            table.Cells[2, 0].TextString = "Машиномест";
            table.Cells[3, 0].TextString = "Машиномест для инвалидов";

            // Машиномест
            table.Cells[2, 1].TextString = data.Places.ToString();
            // Машиномест для инвалидов
            table.Cells[3, 1].TextString = data.InvalidPlaces.ToString();

            cells = CellRange.Create(table, table.Rows.Count-1, 0, table.Rows.Count - 1, table.Columns.Count - 1);
            cells.Borders.Bottom.LineWeight = boldLw;

            table.GenerateLayout();
            return table;
        }

        // вставка
        private void InsertTable(Table table)
        {
            Editor ed = service.Doc.Editor;
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
