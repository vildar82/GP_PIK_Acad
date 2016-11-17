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

        public InsOptionsViewModel (InsOptions opt)
        {
            InsOptions = opt;
            RegionNames = new ObservableCollection<string>(dictRegions.Keys);                        
            SelectedRegionName = InsOptions.Region.RegionName;
            SelectedRegion = opt.Region;

            // Загрузка проектов из базы
            Projects = DBService.GetProjects();            

            // Выбор текущего проекта, если он есть
            if (opt.Project != null && Projects != null && Projects.Any())
            {
                var findProject = Projects.Find(p => p.Id == opt.Project.Id);
                if (findProject != null)
                {
                    SelectedProject = findProject;
                }
            }

            OK = new RelayCommand(OnOkExecute);
        }        

        /// <summary>
        /// Модель
        /// </summary>
        public InsOptions InsOptions { get; set; }        

        public RelayCommand OK { get; set; }

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


        public List<ProjectDB> Projects { get; set; }
        public ProjectDB SelectedProject { get; set; }

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
        }
    }

    public class DesignInsRegionViewModel : InsOptionsViewModel
    {
        public DesignInsRegionViewModel () : base()
        {
        }
    }
}
