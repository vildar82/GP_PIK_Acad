using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Errors;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using PIK_GP_Acad.Elements;

namespace PIK_GP_Acad.FCS
{
    public class FCSTable
    {
        static RXClass RXClassCurve = RXClass.GetClass(typeof(Curve));
        static RXClass RXClassHatch = RXClass.GetClass(typeof(Hatch));

        Document doc;
        Database db;
        Editor ed;
        ITableService tableService;
        IClassTypeService classService;

        public FCSTable (Document doc, ITableService tableService, IClassTypeService classService)
        {
            this.doc = doc;
            db = doc.Database;
            ed = doc.Editor;
            this.tableService = tableService;
            this.classService = classService;
        }

        public void Calc ()
        {
            FCService.Init(db);
            var sel = ed.Select("\nВыбор:");
            var areas = GetAreas(sel);
            // Группировка и суммирование
            CalcData(areas);
            // Таблица            
            var table = tableService.Create();
            tableService.Insert(table, doc);
        }

        private void CalcData (List<IArea> areas)
        {
            var groups = areas.GroupBy(g => g.ClassType.TableName).OrderBy(o => o.First().ClassType.Index).ToList();
            tableService.CalcRows(groups);
        }

        private List<IArea> GetAreas (List<ObjectId> ids)
        {
            var areas = new List<IArea>();
            using (var t = db.TransactionManager.StartTransaction())
            {
                foreach (var idEnt in ids)
                {
                    if (!idEnt.ObjectClass.IsDerivedFrom(RXClassCurve) &&
                        idEnt.ObjectClass != RXClassHatch)
                    {
                        continue;
                    }

                    var ent = idEnt.GetObject(OpenMode.ForRead) as Entity;
                    var area = ElementFactory.Create<IArea>(ent);
                    if (area != null)
                    {                        
                        areas.Add(area);
                    }
                }
                t.Commit();
            }
            return areas;
        }
    }
}
