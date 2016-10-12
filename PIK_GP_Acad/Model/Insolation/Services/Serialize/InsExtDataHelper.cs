﻿using System;
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
        public static Dictionary<string, List<TypedValue>> Load (DBObject dbo, Document doc)
        {
            Dictionary<string, List<TypedValue>> res = null;
            using (doc.LockDocument())
            using (var t = doc.TransactionManager.StartTransaction())
            using (EntDictExt dic = new EntDictExt(dbo, plugin))
            {
                res = dic.LoadAllXRecords();
                t.Commit();
            }
            return res;
        }

        /// <summary>
        /// Сохранение объекта в словарь
        /// </summary>        
        public static void Save (IExtDataSave obj, Document doc)
        {
            using (doc.LockDocument())
            using (var t = doc.TransactionManager.StartTransaction())
            {
                var idDbo = obj.GetDBObject();
                if (!idDbo.IsNull)
                {
                    var dbo = idDbo.GetObject(OpenMode.ForWrite);
                    using (EntDictExt dic = new EntDictExt(dbo, plugin))
                    {
                        dic.Save(obj.GetDataValues(), obj.DataRecName);                        
                    }
                }
                t.Commit();
            }
        }

        /// <summary>
        /// Загрузка из словаря NOD
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="recName"></param>
        public static List<TypedValue> LoadFromNod (Document doc, string recName)
        {
            var nod = new AcadLib.DictNOD(plugin, true);
            var values = nod.Load(recName);
            return values;
        }

        /// <summary>
        /// Сохранение списка значений в словарь NOD чертежа
        /// </summary>
        /// <param name="doc">Документ в который сохранять</param>
        /// <param name="values">Список значений для сохранения</param>
        /// <param name="recName">Ключ имя записи в словаре Xrecord</param>
        public static void SaveToNod (Document doc, List<TypedValue> values, string recName)
        {
            using (doc.LockDocument())
            {
                var nod = new AcadLib.DictNOD(plugin, true);
                nod.Save(values, recName);
            }
        }             
    }
}