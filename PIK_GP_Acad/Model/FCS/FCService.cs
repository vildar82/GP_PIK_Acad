using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using AcadLib.Errors;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.FCS
{
    public static class FCService
    {
        static List<Tuple<ObjectId, string>> tags;
        public static void Init (Database db)
        {
            try
            {


                tags = new List<Tuple<ObjectId, string>>();
                // Чтение всех классификаторов чертежа
                using (var t = db.TransactionManager.StartTransaction())
                {
                    var nod = db.NamedObjectsDictionaryId.GetObject(OpenMode.ForRead) as DBDictionary;
                    if (nod.Contains("ACAD_OC"))
                    {
                        var fcsDict = ((ObjectId)nod["ACAD_OC"]).GetObject(OpenMode.ForRead) as DBDictionary;
                        foreach (var dictSchema in fcsDict)
                        {
                            var fcsGpDict = dictSchema.Value.GetObject(OpenMode.ForRead) as DBDictionary;
                            foreach (var item in fcsGpDict)
                            {
                                var dtItem = item.Value.GetObject(OpenMode.ForRead) as DataTable;
                                for (int r = 0; r < dtItem.NumRows; r++)
                                {
                                    var col = dtItem.GetColumnIndexAtName("isTagged");
                                    var cel = dtItem.GetCellAt(r, col);
                                    var isTagged = (bool)cel.Value;
                                    if (!isTagged)
                                    {
                                        continue;
                                    }
                                    col = dtItem.GetColumnIndexAtName("id");
                                    cel = dtItem.GetCellAt(r, col);
                                    ObjectId idSoft = (ObjectId)cel.Value;
                                    if (idSoft.IsValid || !idSoft.IsNull)
                                    {
                                        tags.Add(new Tuple<ObjectId, string>(idSoft, item.Key));
                                    }
                                }
                            }
                        }
                    }
                    t.Commit();
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, $"FCService Init - {db.Filename}");
                Inspector.AddError($"Ошибка считывания классификации объектов чертежа - {ex.Message}");
            }
        }

        public static List<string> GetAllTags(ObjectId idEnt)
        {
            var res = tags.Where(t => t.Item1 == idEnt).Select(s=>s.Item2).ToList();
            return res;
        }
    }
}
