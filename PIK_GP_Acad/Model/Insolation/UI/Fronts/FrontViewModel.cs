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
            DrawVisuals = new RelayCommand(InsFrontDrawVisuals);
            ShowOptions = new RelayCommand<FrontGroup>(OnShowOptionsExecute);
            ShowHouseOptions = new RelayCommand<House>(OnShowHouseOptionsExecute);
            ClearOverrideOptions = new RelayCommand<House>(OnClearOverrideOptionsExecute);            
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

            // Проверка, что в указанной области есть свободные дома
            if (!Front.Model.Map.Houses.GetHousesInExtents(selReg).Any(h=>h.FrontGroup == null))
            {
                // В области новой группы не должно быть домов из других групп
                InsService.ShowMessage($"В выбранной области нет домов не входящих в другие группы.", System.Windows.MessageBoxImage.Error);
                return;
            }

            // Создание группы фронтонов
            try
            {                
                var frontGroup = FrontGroup.New(selReg, Front);

                // Диалог настроек фронта
                var fgOptionsVM = new FrontGroupOptionsViewModel(frontGroup.Options, false, frontGroup.FrontLevel);
                if (InsService.ShowDialog(fgOptionsVM) == true)
                {
                    frontGroup.FrontLevel = fgOptionsVM.FrontLevel;
                    frontGroup.IsInitialized = true;
                    Front.AddGroup(frontGroup);
                    // Запись статистики
                    PluginStatisticsHelper.AddStatistic();
                }                
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
            if (house == null) return;
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
        private void InsFrontDrawVisuals()
        {
            Front.DrawVisuals();
            PluginStatisticsHelper.AddStatistic();
        }

        private void OnShowOptionsExecute(FrontGroup group)
        {
            var fgOptionsVM = new FrontGroupOptionsViewModel(group.Options, true, group.FrontLevel);
            if (InsService.ShowDialog(fgOptionsVM) == true)
            {
                if (fgOptionsVM.SelectedExtents != null)
                {
                    group.SelectRegion = fgOptionsVM.SelectedExtents.Value;
                }
                group.Update();
            }
        }

        private void OnShowHouseOptionsExecute(House house)
        {
            HouseOptions houseOptions = house.Options ?? new HouseOptions(house.FrontGroup.Options);            

            var fgOptionsVM = new FrontGroupOptionsViewModel(houseOptions, false, house.FrontLevel);
            if (InsService.ShowDialog(fgOptionsVM) == true)
            {                
                house.Options = houseOptions;
                if (house.FrontLevel != fgOptionsVM.FrontLevel)
                {
                    house.FrontLevel = fgOptionsVM.FrontLevel; // Будет обновлен прасчет
                }
                else
                {
                    house.Update();
                }
            }
        }
        
        private void OnClearOverrideOptionsExecute(House house)
        {
            house.Options = null;
            house.Update();
        }        
    }
}
