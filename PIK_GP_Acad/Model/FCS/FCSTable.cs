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
            var sel = ed.Select("\nВыбор:");
            var classifivators = GetClassificators(sel);
            // Группировка и суммирование
            CalcData(classifivators);
            // Таблица            
            var table = tableService.Create();
            tableService.Insert(table, doc);
        }

        private void CalcData (List<IClassificator> classificators)
        {
            var groups = classificators.GroupBy(g => g.ClassType.TableName).OrderBy(o => o.First().ClassType.Index).ToList();
            tableService.CalcRows(groups);
        }

        private List<IClassificator> GetClassificators (List<ObjectId> ids)
        {
            List<IClassificator> classificators = new List<IClassificator>();
            using (var t = db.TransactionManager.StartTransaction())
            {
                foreach (var idEnt in ids)
                {
                    if (!idEnt.ObjectClass.IsDerivedFrom(RXClassCurve) &&
                        idEnt.ObjectClass != RXClassHatch)
                    {
                        continue;
                    }


                    KeyValuePair<string, List<FCProperty>> tag;
                    if(FCService.GetTag(idEnt, out tag))                    
                    {
                        var classificator = ClassFactory.Create(idEnt, tag.Key, classService);
                        if (classificator == null)
                        {
                            Inspector.AddError($"Пропущен объект класса - {string.Join(",", tag.Key)}",
                                idEnt, System.Drawing.SystemIcons.Warning);
                        }
                        else
                        {
                            classificators.Add(classificator);
                        }
                    }
                }
                t.Commit();
            }
            return classificators;
        }
    }
}
