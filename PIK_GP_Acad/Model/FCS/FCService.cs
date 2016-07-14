using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.FCS
{
    public static class FCService
    {
        static List<Tuple<ObjectId, string>> tags;
        public static void Init(Database db)
        {
            tags = new List<Tuple<ObjectId, string>>();
            // Чтение всех классификаторов чертежа
            using (var t = db.TransactionManager.StartTransaction())
            {   
                var nod = db.NamedObjectsDictionaryId.GetObject(OpenMode.ForRead) as DBDictionary;
                if (nod.Contains("ACAD_OC"))
                {
                    var fcsDict = ((ObjectId)nod["ACAD_OC"]).GetObject(OpenMode.ForRead) as DBDictionary;
                    if (fcsDict.Contains("GP"))
                    {
                        var fcsGpDict = ((ObjectId)fcsDict["GP"]).GetObject(OpenMode.ForRead) as DBDictionary;
                        foreach (var item in fcsGpDict)
                        {                            
                            var dtItem = item.Value.GetObject(OpenMode.ForRead) as DataTable;
                            for (int c = 0; c < dtItem.NumColumns; c++)
                            {
                                var col = dtItem.GetColumnAt(c);
                                if (col.ColumnName.Equals("id", StringComparison.OrdinalIgnoreCase))
                                {
                                    for (int r = 0; r < dtItem.NumRows; r++)
                                    {
                                        var cel = col.GetCellAt(r);
                                        ObjectId idSoft = (ObjectId)cel.Value;
                                        tags.Add(new Tuple<ObjectId, string>(idSoft, item.Key));
                                    }                                    
                                }
                            }                            
                        }
                    }
                }
                t.Commit();
            }
        }

        public static List<string> GetAllTags(ObjectId idEnt)
        {
            var res = tags.Where(t => t.Item1 == idEnt).Select(s=>s.Item2).ToList();
            return res;
        }
    }
}
