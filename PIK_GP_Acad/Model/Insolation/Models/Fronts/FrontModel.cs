using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using AcadLib.XData;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using MicroMvvm;
using PIK_GP_Acad.Insolation.Services.Export;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Фронтоны - модель расчета
    /// </summary>
    public class FrontModel : ModelBase, IExtDataSave, ITypedDataValues
    {
        public FrontModel ()
        {
            Groups = new ObservableCollection<FrontGroup>();
        }

        public InsModel Model { get; set; }

        public FrontOptions Options { get; set; }

        /// <summary>
        /// Группы - выбранные области на чертеже для расчета фронтонов
        /// </summary>
        public ObservableCollection<FrontGroup> Groups {
            get { return groups; }
            set { groups = value; RaisePropertyChanged(); }
        }
        ObservableCollection<FrontGroup> groups;

        public bool IsVisualsFrontOn { get { return isVisualsFrontOn; } set { isVisualsFrontOn = value; RaisePropertyChanged(); } }
        bool isVisualsFrontOn;


        /// <summary>
        /// Инициализация расчета
        /// </summary>        
        public void Initialize (InsModel insModel)
        {
            this.Model = insModel;

            // Настройки
            if (Options == null)
            {
                Options = FrontOptions.Default();
            }
        }

        /// <summary>
        /// Добавление новой группы пользователем
        /// </summary>        
        public void AddGroup (FrontGroup group)
        {
            Groups.Add(group);
            // Обновлоение расчета группы
            group.Update();
        }

        /// <summary>
        /// Добавление группы из словаря
        /// </summary>        
        private void AddGroup (DicED dicGroup)
        {
            var group = FrontGroup.New(dicGroup, this);
            Groups.Add(group);
            //group.Update();
        }

        public void DeleteGroup (FrontGroup group)
        {
            Groups.Remove(group);
            group.Dispose();
        }

        public List<TypedValue> GetDataValues (Document doc)
        {
            return new List<TypedValue>() {
                TypedValueExt.GetTvExtData(IsVisualsFrontOn)
            };
        }

        public void SetDataValues (List<TypedValue> values, Document doc)
        {
            if (values == null || values.Count != 1)
            {
                // Default
                IsVisualsFrontOn = true;
            }
            else
            {
                int index = 0;
                IsVisualsFrontOn = values[index++].GetTvValue<bool>();
            }
        }

        public DicED GetExtDic (Document doc)
        {
            DicED dicFront = new DicED();

            // Список значений расчета елочек                              
            dicFront.AddRec(new RecXD("FrontModelRec", GetDataValues(doc)));

            // Сохранение настроек            
            dicFront.AddInner("FrontOptions", Options.GetExtDic(doc));

            // Сохранение групп
            int groupCount = 0;
            foreach (var group in Groups)
            {
                dicFront.AddInner("Group" + groupCount, group.GetExtDic(doc));
                groupCount++;
            }

            // Сохранение имен домов в блок-секциях
            using (doc.LockDocument())
            using (var t = doc.TransactionManager.StartTransaction())
            {
                var sections = Groups.SelectMany(s => s.Houses.SelectMany(h => h.Sections));
                foreach (var item in sections)
                {
                    item.Building.SaveDboDict();
                }
                t.Commit();
            }

            return dicFront;
        }

        public void SetExtDic (DicED dicFront, Document doc)
        {
            if (dicFront == null)
            {
                // Default                
                return;
            }
            // Собственные значения рассчета
            SetDataValues(dicFront.GetRec("FrontModelRec")?.Values, doc);
            // настроки
            Options = new FrontOptions();
            Options.SetExtDic(dicFront.GetInner("FrontOptions"), doc);

            int groupCount = 0;
            DicED dicGroup;
            do
            {
                dicGroup = dicFront.GetInner("Group" + groupCount++);
                if (dicGroup != null)
                {
                    AddGroup(dicGroup);
                }
            } while (dicGroup != null);
        }

        public void Update ()
        {
            foreach (var item in groups)
            {
                item.Update();
            }
        }

        public void ClearVisual ()
        {
            foreach (var item in Groups)
            {
                item.ClearVisual();
            }
        }

        public void UpdateVisual ()
        {
            foreach (var item in Groups)
            {
                item.UpdateVisual();
            }
        }

        /// <summary>
        /// Экпорт инсоляции в базу
        /// </summary>
        public void ExportInsToBD ()
        {
            var export = new ExportToDB(this);
            export.Export();
        }
    }
}