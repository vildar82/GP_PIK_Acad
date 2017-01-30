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
        /// Список id корпусов (зданий) из базы для текущего проекта.
        /// Единая коллекция из которой выбираются дома для связывания.
        /// Дома связанные дважды с разными домами на чертеже, подсвечиваются Красным,
        /// Уже связанные дома - подсвечиваются Зеленым,
        /// Свободные дома для связывания - не подсвечены цветом.
        /// </summary>
        public ObservableCollection<HouseDbSel> HousesDb { get { return housesDb; } set { housesDb = value; RaisePropertyChanged(); } }
        ObservableCollection<HouseDbSel> housesDb;

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

            //// Сохранение имен домов в блок-секциях
            //using (doc.LockDocument())
            //using (var t = doc.TransactionManager.StartTransaction())
            //{
            //    var sections = Groups.SelectMany(s => s.Houses.SelectMany(h => h.Sections));
            //    foreach (var item in sections)
            //    {
            //        item.Building.SaveDboDict();
            //    }
            //    t.Commit();
            //}

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
        /// Определение связанных/несвязанных корпусов проекта и
        /// </summary>
        public void DefineHouseDb()
        {
            // Проект
            if (HousesDb != null)
            {
                HousesDb.Clear();
                HousesDb = null;
            }
            var project = Model.Options.Project;
            var houses = Groups.SelectMany(s => s.Houses.Select(h => h)).ToList();            
            if (project == null)
            {                
                foreach (var item in houses)
                {
                    item.HouseId = 0;
                }
            }
            else
            {
                // Корпуса (дома)  в проекте                
                var objHousesDb = DbService.GetHouses(project);
                HousesDb = new ObservableCollection<HouseDbSel>();
                if (objHousesDb != null)
                {
                    foreach (var item in objHousesDb)
                    {
                        var houseDbSel = new HouseDbSel(item);
                        HousesDb.Add(houseDbSel);
                    }
                    HousesDb.Add(new HouseDbSel(null));
                }
                                
                // определение уже связанных домов
                if (HousesDb.Any())
                {
                    foreach (var item in HousesDb)
                    {
                        item.ClearConnections();
                    }
                    foreach (var item in houses)
                    {
                        if (item.HouseId != 0)
                        {
                            var houseDbSel = HousesDb.SingleOrDefault(h => h.Id == item.HouseId);
                            if (houseDbSel == null)
                            {
                                // Назначенный дому id не найден - обнуление
                                item.HouseId = 0;
                            }
                            else
                            {
                                houseDbSel.Connect(item);
                            }
                        }
                    }                    
                }
            }
        }

        public HouseDbSel FindHouseDb(int houseId)
        {            
            var resHouseDbsel = HousesDb?.SingleOrDefault(h => h.Id == houseId);
            return resHouseDbsel;
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
                foreach (var item in group.Houses)
                {
                    item.VisualFront.DrawForUser();
                }
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