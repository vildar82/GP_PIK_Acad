﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.Insolation.Services;
using AcadLib;
using AcadLib.XData;
using MicroMvvm;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Настройки елочек
    /// </summary>
    public class TreeOptions : ModelBase, IExtDataSave, ITypedDataValues
    {
        /// <summary>
        /// Список высотностей для елочек
        /// Должны быть отсортированы по возрастанию высоты и не должно быть повторений высот
        /// </summary>
        public ObservableCollection<TreeVisualOption> TreeVisualOptions { get; set; }
        /// <summary>
        /// Прозрачность
        /// </summary>
        public byte Transparence { get { return transparence; } set { transparence = value; } }
        byte transparence;

        public static TreeOptions Default ()
        {
            TreeOptions defTreeOpt = new TreeOptions();
            defTreeOpt.TreeVisualOptions = new ObservableCollection<TreeVisualOption>(TreeVisualOption.DefaultTreeVisualOptions());
            defTreeOpt.Transparence = 60;
            return defTreeOpt;
        }

        /// <summary>
        /// Создание словаря для сохранение этого объекта
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>Словарь для сохранения</returns>
        public DicED GetExtDic (Document doc)
        {
            DicED dicTreeOpt = new DicED();
            // Записи виз настроек - для каждой высоты своя запись XRecord в словаре
            var dicTreeVisualOpt = new DicED("TreeVisualOptions");
            foreach (var item in TreeVisualOptions)
            {                
                var values = item.GetDataValues(doc);
                string name = item.Height.ToString();
                dicTreeVisualOpt.AddRec(name, values);
            }
            dicTreeOpt.AddInner(dicTreeVisualOpt);
            // Transparence
            dicTreeOpt.AddRec("Rec", GetDataValues(doc));
            return dicTreeOpt;
        }

        /// <summary>
        /// Установка значений из словаря
        /// </summary>
        public void SetExtDic (DicED dicTreeOpt, Document doc)
        {
            if (dicTreeOpt == null)
            {
                // Default
                TreeVisualOptions = new ObservableCollection<TreeVisualOption>(TreeVisualOption.DefaultTreeVisualOptions());
                SetDataValues(null, doc);
                return;
            }
            // Список настроек
            SetDataValues(dicTreeOpt.GetRec("Rec")?.Values, doc);

            // Список настроек визуалных высот
            var treeVisOpts = new List<TreeVisualOption>();
            var dicTreeVisualOpt = dicTreeOpt.GetInner("TreeVisualOptions");
            if (dicTreeVisualOpt != null)
            {
                foreach (var item in dicTreeVisualOpt.Recs)
                {
                    var treeVis = new TreeVisualOption();
                    treeVis.SetDataValues(item.Values, doc);
                    treeVisOpts.Add(treeVis);
                }
            }
            // Проверка высот
            TreeVisualOption.CheckAndCorrect(ref treeVisOpts);
            TreeVisualOptions = new ObservableCollection<TreeVisualOption>(treeVisOpts);            
        }

        public List<TypedValue> GetDataValues(Document doc)
        {
            var tvk = new TypedValueExtKit();
            tvk.Add("Transparence", Transparence);            
            return tvk.Values;            
        }

        public void SetDataValues(List<TypedValue> values, Document doc)
        {
            var dictValues = values?.ToDictionary();
            Transparence = dictValues.GetValue("Transparence", (byte)60);            
        }
    }
}
