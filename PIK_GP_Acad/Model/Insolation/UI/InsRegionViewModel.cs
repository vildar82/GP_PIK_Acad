using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections.ObjectModel;
using PIK_GP_Acad.Insolation.Models;
using MicroMvvm;

namespace PIK_GP_Acad.Insolation.UI
{
    public class InsRegionViewModel : ViewModelBase
    {
        Dictionary<string, ObservableCollection<InsRegion>> dictRegions = Services.InsService.Settings.Regions.
                     GroupBy(g => g.RegionName).OrderBy(o => o.Key).ToDictionary(k => k.Key, v =>
                     {
                         var regs = new ObservableCollection<InsRegion>();
                         foreach (var item in v)
                         {
                             regs.Add(item);
                         }
                         return regs;
                     });

        public InsRegionViewModel ()
        {

        }

        public InsRegionViewModel (InsRegion region)
        {
            InsRegion = region;
            RegionNames = new ObservableCollection<string>(dictRegions.Keys);                        
            SelectedRegionName = InsRegion.RegionName;
            SelectedRegion = region;

            OK = new RelayCommand(OnOkExecute);
        }        

        /// <summary>
        /// Модель
        /// </summary>
        public InsRegion InsRegion { get; set; }

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
            InsRegion = SelectedRegion;
        }
    }

    public class DesignInsRegionViewModel : InsRegionViewModel
    {
        public DesignInsRegionViewModel () : base()
        {
        }
    }
}
