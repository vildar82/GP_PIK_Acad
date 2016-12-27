using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using AcadLib.Errors;
using Autodesk.AutoCAD.DatabaseServices;
using NetLib;

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
        static Dictionary<ObjectId, FCEntProps> tags = new Dictionary<ObjectId,FCEntProps>();
        public static void Init (Database db)
        {
            try
            {
                tags = new Dictionary<ObjectId, FCEntProps>();
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
                                        if (!tags.ContainsKey(idSoft))
                                        {
                                            tags.Add(idSoft, new FCEntProps(item.Key, idSoft, props));
                                        }                                                                                
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
            var props = new List<FCProperty>();

            for (int c = 0; c < dtItem.NumColumns; c++)
            {
                var col = dtItem.GetColumnNameAt(c);
                if (col == "id" || col == "isTagged") continue;
                var cel = dtItem.GetCellAt(r, c);
                var prop = new FCProperty(col, cel.Value);
                props.Add(prop);
            }
            return props;
        }

        /// <summary>
        /// Получение классификации объекта - имени класса и свойств
        /// Перед использованием нужно первый раз вызвать Init для сканирования объектов чертежа
        /// </summary>        
        public static bool GetEntityProperties (ObjectId idEnt, out FCEntProps tag)
        {
            bool foundTag = false;
            if (tags.TryGetValue(idEnt, out tag))
            {
                foundTag = true;
            }
            return foundTag;
        }

        public static T GetPropertyValue<T> (this List<FCProperty> props, string name, ObjectId idEnt, bool isRequired, T defaultValue)
        {
            T resVal = default(T);
            var prop = props.Find(p => p.Name.EqualsIgroreCaseAndSpecChars(name));
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
                    resVal = prop.Value.GetValue<T>();// (T)Convert.ChangeType(prop.Value, typeof(T));
                }
                catch (Exception ex)
                {
                    var err = $"Недопустимый тип значения параметра '{name}'= {prop.Value.ToString()}";

                    if (isRequired)
                    {
                        Inspector.AddError(err,idEnt, System.Drawing.SystemIcons.Error);
                    }
                    else
                    {
                        Logger.Log.Error(ex, err);
                    }
                    resVal = defaultValue;
                }
            }
            return resVal;
        }
    }
}
