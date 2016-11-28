using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections.ObjectModel;
using PIK_GP_Acad.Insolation.Models;
using MicroMvvm;
using PIK_DB_Projects;
using AcadLib;
using PIK_GP_Acad.Insolation.Services;

namespace PIK_GP_Acad.Insolation.UI
{
    public class InsOptionsViewModel : ViewModelBase
    {
        Dictionary<string, ObservableCollection<InsRegion>> dictRegions = InsService.Settings.Regions.
                     GroupBy(g => g.RegionName).OrderBy(o => o.Key).ToDictionary(k => k.Key, v =>
                     {
                         var regs = new ObservableCollection<InsRegion>();
                         foreach (var item in v)
                         {
                             regs.Add(item);
                         }
                         return regs;
                     });

        public InsOptionsViewModel ()
        {
            
        }

        public InsOptionsViewModel (InsModel insModel)
        {
            InsModel = insModel;
            InsOptions =insModel.Options;
            RegionNames = new ObservableCollection<string>(dictRegions.Keys);
            // Загрузка проектов из базы
            Projects = DbService.GetProjects();

            // Установка свойств настроек
            FillProperties(InsOptions);

            OK = new RelayCommand(OnOkExecute);
            Reset = new RelayCommand(OnResetExecute);
        }        

        public InsModel InsModel { get; set;}

        /// <summary>
        /// Модель
        /// </summary>
        public InsOptions InsOptions { get; set; }        

        public RelayCommand OK { get; set; }
        public RelayCommand Reset { get; set; }

        public ObservableCollection<string> RegionNames { get; set; }
        public string SelectedRegionName {
            get { return selectedRegionName; }
            set { selectedRegionName = value; RaisePropertyChanged(); OnSelectedRegionNameChanged(); }
        }
        string selectedRegionName;

        public ObservableCollection<InsRegion> Cities { get{ return cities; } set { cities = value; RaisePropertyChanged(); } }
        ObservableCollection<InsRegion> cities;

        public InsRegion SelectedRegion { get { return selectedRegion; } set { selectedRegion = value; RaisePropertyChanged(); } }
        InsRegion selectedRegion;

        public List<ProjectMDM> Projects { get; set; }
        public ProjectMDM SelectedProject { get; set; }

        private void OnSelectedRegionNameChanged ()
        {
            FillCityes();
        }
        private void FillCityes ()
        {
            if (string.IsNullOrEmpty(SelectedRegionName))
            {
                Cities = null;
            }
            else
            {
                Cities = dictRegions[SelectedRegionName];
                SelectedRegion = Cities[0];
            }                
        }

        private void OnOkExecute ()
        {
            InsOptions.Region = SelectedRegion;
            if (SelectedProject != null)
            {
                InsOptions.Project = SelectedProject;
            }
            InsModel.Options = InsOptions;
        }

        private void OnResetExecute()
        {
            var defaultOpt =InsOptions.Default();
            FillProperties(InsOptions);
        }

        private void FillProperties(InsOptions opt)
        {
            SelectedRegionName = InsOptions.Region.RegionName;
            SelectedRegion = opt.Region;

            // Выбор текущего проекта, если он есть
            if (opt.Project != null && Projects != null && Projects.Any())
            {
                var findProject = Projects.Find(p => p.Id == opt.Project.Id);
                if (findProject != null)
                {
                    SelectedProject = findProject;
                }
            }
        }
    }

    public class DesignInsRegionViewModel : InsOptionsViewModel
    {
        public DesignInsRegionViewModel () : base()
        {
        }
    }
}