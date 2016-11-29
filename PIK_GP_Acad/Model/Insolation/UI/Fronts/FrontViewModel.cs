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

namespace PIK_GP_Acad.Insolation.UI
{
    public class FrontViewModel : ViewModelBase
    {
        public FrontViewModel (FrontModel model)
        {
            Front = model;
            Add = new RelayCommand(OnAddExecute);
            Delete = new RelayCommand<FrontGroup>(OnDeleteExecute);
            ShowHouse = new RelayCommand<House>(OnShowHouseExecute);

            FillHouseDb();
        }        

        /// <summary>
        /// Модель
        /// </summary>
        public FrontModel Front { get; set; }        

        public RelayCommand Add { get; set; }
        public RelayCommand<FrontGroup> Delete { get; set; }
        public RelayCommand<House> ShowHouse { get; set; }        

        public Visibility ComboboxHouseDbVisibility 
            { get { return comboboxHouseDbVisibility; }
            set { comboboxHouseDbVisibility = value; RaisePropertyChanged(); } }
        Visibility comboboxHouseDbVisibility;        

        private void OnAddExecute ()
        {
            // Выбор области на чертеже
            var selectGroup = new SelectGroup(Front.Model.Doc);
            Extents3d selReg;
            try
            {
                selReg = selectGroup.Select();
            }
            catch {
                return;
            }
            // Создание группы фронтонов
            try
            {                
                var frontGroup = FrontGroup.New(selReg, Front);
                // Включение расчета группы
                frontGroup.IsVisualFrontOn = true;
                Front.AddGroup(frontGroup);
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
            house.Show();
        }        

        private void FillHouseDb()
        {
            var project = Front.Model.Options.Project;
            if (project == null)
            {
                ComboboxHouseDbVisibility = Visibility.Invisible;                
            }
            else
            {
                ComboboxHouseDbVisibility = Visibility.Visible;                                
            }
        }
    }
}
