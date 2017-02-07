﻿using System;
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
using PIK_DB_Projects;
using PIK_GP_Acad.Insolation.Services;
using PIK_GP_Acad.Insolation.UI;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Фронтоны - модель расчета
    /// </summary>
    public class FrontModel : ModelBase, IExtDataSave, ITypedDataValues, IDisposable
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
            if (string.IsNullOrEmpty(group.Name))
            {
                group.DefineNewName();
            }
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
            return null;
        }

        public void SetDataValues (List<TypedValue> values, Document doc)
        {
            // нет параметров            
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

            return dicFront;
        }        

        public void SetExtDic (DicED dicFront, Document doc)
        {
            if (dicFront == null)
            {
                // Default                
                return;
            }
            // Собственные значения расчета
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
            // Определение корпусов проекта
            //DefineHouseDb();
            // Расчет групп фронтов
            if (Groups != null)
            {
                foreach (var item in Groups)
                {   
                    item?.Update();
                }
            }    
        }

        public void ClearVisual ()
        {
            if (Groups == null) return;
            foreach (var item in Groups)
            {
                item?.ClearVisual();
            }
        }

        public void UpdateVisual ()
        {
            if (Groups == null) return;
            foreach (var item in Groups)
            {
                item?.UpdateVisual();
            }
        }     

        /// <summary>
        /// Экпорт инсоляции в базу
        /// </summary>
        public void Export ()
        {
            if (Groups.Any())
            {
                var export = new ExportToDB(this);
                export.Export();
            }
        }

        /// <summary>
        /// Рисование визуализации на чертеже
        /// </summary>
        public void DrawVisuals()
        {
            foreach (var group in Groups)
            {
                var viuals = new List<IVisualService>();
                foreach (var item in group.Houses)
                {
                    viuals.Add(item.VisualFront);                    
                }
                VisualDatabaseAny.DrawVisualsForUser(viuals);
            }
        }

        public void Dispose()
        {
            if (Groups!= null)
            {
                foreach (var item in Groups)
                {
                    item.Dispose();
                }
            }
        }
    }
}