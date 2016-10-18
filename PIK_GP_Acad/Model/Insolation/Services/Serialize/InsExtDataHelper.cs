using System;
using System.Collections.Generic;
using AcadLib.XData;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.Insolation.Services
{
    /// <summary>
    /// Загрузка/сохранение объектов инсоляции из объектов чертежа (расширенных данных)
    /// </summary>
    public static class InsExtDataHelper
    {
        private const string plugin = "Insolation";

        /// <summary>
        /// Загрузка словаря из объекта - всех записей Xrecord
        /// </summary>                
        public static DicED Load (DBObject dbo, Document doc)
        {
            DicED res = null;
            EntDictExt ede = new EntDictExt(dbo, plugin);
            res = ede.Load();
            return res;
        }

        /// <summary>
        /// Сохранение объекта в словарь
        /// </summary>        
        public static void Save (IDboDataSave obj, Document doc)
        {
            if (doc == null || doc.IsDisposed) return;
                    
            var dicEd = obj.GetExtDic(doc);
            if (dicEd == null) return;

            using (doc.LockDocument())
            using (var t = doc.TransactionManager.StartTransaction())
            {
                var idDbo = obj.GetDBObject();
                if (!idDbo.IsNull)
                {
                    var dbo = idDbo.GetObject(OpenMode.ForWrite);
                    if (dbo != null)
                    {
                        EntDictExt ede = new EntDictExt(dbo, plugin);
                        ede.Save(dicEd);
                    }
                }
                t.Commit();
            }
        }

        /// <summary>
        /// Загрузка из словаря NOD
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="dicName">Имя словаря объекта</param>
        public static DicED LoadFromNod (Document doc, string dicName)
        {
            using (doc.LockDocument())
            {
                var nod = new AcadLib.DictNOD(plugin, true);
                nod.Db = doc.Database;
                var DicED = nod.LoadED(dicName);
                return DicED;
            }
        }

        /// <summary>
        /// Сохранение списка значений в словарь NOD чертежа
        /// </summary>
        /// <param name="doc">Документ в который сохранять</param>
        /// <param name="DicED">Список значений для сохранения</param>        
        public static void SaveToNod (Document doc, DicED DicED)
        {
            if (doc == null || doc.IsDisposed) return;
            using (doc.LockDocument())
            {
                var nod = new AcadLib.DictNOD(plugin, true);
                nod.Db = doc.Database;
                nod.Save(DicED);
            }
        }             
    }
}