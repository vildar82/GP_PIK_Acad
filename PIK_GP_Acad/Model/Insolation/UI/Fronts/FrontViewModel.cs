using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using MicroMvvm;
using PIK_GP_Acad.Insolation.Models;
using PIK_GP_Acad.Insolation.Services;
using System.Collections.ObjectModel;
using PIK_DB_Projects;
using AcadLib.Statistic;
using Autodesk.AutoCAD.ApplicationServices;

namespace PIK_GP_Acad.Insolation.UI
{
    public class FrontViewModel : ViewModelBase
    {
        public FrontViewModel (FrontModel model)
        {
            Front = model;
            Add = new RelayCommand(InsAddFrontExecute);
            Delete = new RelayCommand<FrontGroup>(OnDeleteExecute);
            ShowHouse = new RelayCommand<House>(OnShowHouseExecute);
            Export = new RelayCommand(InsFrontExportExecute);
            DrawVisuals = new RelayCommand(InsFrontDrawVisualsExecute);
            ShowOptions = new RelayCommand<FrontGroup>(OnShowOptionsExecute);
            ShowHouseOptions = new RelayCommand<House>(OnShowHouseOptionsExecute);
            ClearOverrideOptions = new RelayCommand<House>(OnClearOverrideOptionsExecute);
            FillHouseDb();
        }

        /// <summary>
        /// Модель
        /// </summary>
        public FrontModel Front { get; set; }        

        public RelayCommand Add { get; set; }
        public RelayCommand<FrontGroup> Delete { get; set; }
        public RelayCommand<House> ShowHouse { get; set; }
        public RelayCommand Export { get; set; }
        public RelayCommand DrawVisuals { get; set; }
        public RelayCommand<FrontGroup> ShowOptions { get; set; }
        public RelayCommand<House> ShowHouseOptions { get; set; }
        public RelayCommand<House> ClearOverrideOptions { get; set; }
        
        public bool HasProject { get; set; }         

        private void InsAddFrontExecute ()
        {            
            // Выбор области на чертеже
            var selectGroup = new SelectGroup(Front.Model.Doc);
            Extents3d selReg;
            try
            {
                selReg = selectGroup.Select();
            }
            catch
            {
                return;
            }

            // Проверка, что в указанной области нет домов из других групп
            if (Front.Model.Map.Houses.GetHousesInExtents(selReg).Any(h=>h.FrontGroup != null))
            {
                // В области новой группы не должно быть домов из других групп
                InsService.ShowMessage($"В выбранной области не должно быть домов входящих в другие группы.", System.Windows.MessageBoxImage.Error);
                return;
            }

            // Создание группы фронтонов
            try
            {                
                var frontGroup = FrontGroup.New(selReg, Front);
                // Включение расчета группы
                frontGroup.IsVisualFrontOn = true;
                Front.AddGroup(frontGroup);
                // Запись статистики
                PluginStatisticsHelper.AddStatistic();
            }
            catch(Exception ex)
            {
                InsService.ShowMessage(ex, "Ошибка при создании группы фронтонов.");
            }
        }

        private void OnDeleteExecute (FrontGroup group)
        {
            Front.DeleteGroup(group);
        }

        private void OnShowHouseExecute (House house)
        {
            if ((short)Application.GetSystemVariable("TILEMODE") == 0) return;
            house.Show();
        }

        private void InsFrontExportExecute()
        {
            // Экспорт фронтов инсоляции
            Front.Export();
            PluginStatisticsHelper.AddStatistic();
        }

        /// <summary>
        /// Рисование визуализации в чертеже
        /// </summary>
        private void InsFrontDrawVisualsExecute()
        {
            Front.DrawVisuals();
            PluginStatisticsHelper.AddStatistic();
        }

        private void OnShowOptionsExecute(FrontGroup group)
        {
            var fgOptionsVM = new FrontGroupOptionsViewModel(group.Options);
            if (InsService.ShowDialog(fgOptionsVM) == true)
            {
                group.Update();
            }
        }

        private void OnShowHouseOptionsExecute(House house)
        {
            HouseOptions houseOptions = house.Options ?? new HouseOptions(house.FrontGroup.Options);            

            var fgOptionsVM = new FrontGroupOptionsViewModel(houseOptions);
            if (InsService.ShowDialog(fgOptionsVM) == true)
            {
                if (!houseOptions.Equals(house.FrontGroup.Options))
                {
                    house.Options = houseOptions;
                    house.Update(house.FrontGroup.Houses.IndexOf(house) + 1);
                }
            }
        }
        
        private void OnClearOverrideOptionsExecute(House house)
        {
            house.Options = null;
        }

        private void FillHouseDb()
        {
            var project = Front?.Model?.Options?.Project;
            HasProject = project != null;            
        }
    }
}
