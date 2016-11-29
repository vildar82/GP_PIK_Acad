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

        public ObservableCollection<ObjectMDM> HousesDbFree { get { return housesDb; } set { housesDb = value; RaisePropertyChanged(); } }
        ObservableCollection<ObjectMDM> housesDb;

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
            };
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
            // Определение корпусов проекта
            DefineHouseDb();
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
            var project = Model.Options.Project;
            var houses = Groups.SelectMany(s => s.Houses.Select(h => h)).ToList();
            HousesDbFree = null;
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
                var housesDb = DbService.GetHouses(project);
                // Исключение из корпусов уже назначенных
                if (housesDb != null)
                {
                    foreach (var item in houses)
                    {
                        if (item.HouseId != 0)
                        {
                            var itemDb = housesDb.FirstOrDefault(h => h.Id == item.HouseId);
                            if (itemDb == null)
                            {
                                // Назначенный дому id не найден - обнуление
                                item.HouseId = 0;
                            }
                            else
                            {
                                housesDb.Remove(itemDb);
                            }
                        }
                    }
                    if (housesDb != null)
                    {
                        HousesDbFree = new ObservableCollection<ObjectMDM>(housesDb);
                        housesDb.Add(null); // Пустой объект - чтобы отменить назнваченный выбор
                    }
                }
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