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
        /// <summary>
        /// Классы объектов в чертеже и их свойства 
        /// dict key (ObjectId) - объект чертежа, 
        /// value key (string) - имя класса,
        /// value (List FCProperty) - свойства класса объекта
        /// </summary>
        static Dictionary<ObjectId, KeyValuePair<string, List<FCProperty>>> tags;
        public static void Init (Database db)
        {
            try
            {
                tags = new Dictionary<ObjectId, KeyValuePair<string, List<FCProperty>>>();
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

                                    // остальные свойства
                                    List<FCProperty> props = GetProperties(dtItem, r);

                                    if (idSoft.IsValid || !idSoft.IsNull)
                                    {
                                        tags.Add(idSoft, new KeyValuePair<string, List<FCProperty>> (item.Key, props));
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

        private static List<FCProperty> GetProperties (DataTable dtItem, int r)
        {
            List<FCProperty> props = new List<FCProperty>();

            for (int c = 0; c < dtItem.NumColumns; c++)
            {
                var col = dtItem.GetColumnNameAt(c);
                if (col == "id" || col == "isTagged") continue;
                var cel = dtItem.GetCellAt(r, c);
                FCProperty prop = new FCProperty(col, cel.Value);
                props.Add(prop);
            }
            return props;
        }

        public static bool GetTag(ObjectId idEnt, out KeyValuePair<string, List<FCProperty>> tag)
        {
            bool foundTag = false;     
            if (tags.TryGetValue(idEnt, out tag))
            {
                foundTag = true;                                
            }
            return foundTag;
        }

        public static T GetPropertyValue<T> (string name, List<FCProperty> props, ObjectId idEnt, bool isRequired)
        {
            T resVal = default(T);
            var prop = props.Find(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (prop == null)
            {
                if (isRequired)
                {
                    Inspector.AddError($"Не определен параметр {name}", idEnt, System.Drawing.SystemIcons.Error);
                }
            }
            else
            {
                try
                {
                    resVal = (T)Convert.ChangeType(prop.Value, typeof(T));
                }
                catch
                {
                    Inspector.AddError($"Недопустимый тип значения параметра '{name}'= {prop.Value.ToString()}.",
                        idEnt, System.Drawing.SystemIcons.Error);
                }
            }
            return resVal;
        }
    }
}
