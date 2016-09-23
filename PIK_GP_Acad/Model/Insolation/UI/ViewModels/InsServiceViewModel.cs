using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using MicroMvvm;
using PIK_GP_Acad.Insolation.Options;
using PIK_GP_Acad.Insolation.UI.Trees;

namespace PIK_GP_Acad.Insolation.UI
{
    public class InsServiceViewModel : ObservableObject
    {        
        Dictionary<string, List<RegionOptions>> dictRegions = InsOptions.Regions.
            GroupBy(g => g.RegionName).OrderBy(o => o.Key).ToDictionary(k => k.Key, v => v.ToList());
        string _selectedRegionName;
        RegionOptions _selectedRegion;
        List<RegionOptions> _cities;
        public IInsolationService InsService { get; set; }
        public Document Doc { get; set; }

        public InsServiceViewModel(Document doc)
        {            
            RegionNames = dictRegions.Keys.ToList();
            Doc = doc;
        }

        public ObservableCollection<IInsCalcViewModel> Tabs { get; set; } = 
            new ObservableCollection<IInsCalcViewModel> { new TreesViewModel() };

        public IInsCalcViewModel SelectedTab {
            set { value.Update(SelectedRegion); }
        }

        public List<string> RegionNames { get; set; }

        public string SelectedRegionName {
            get { return _selectedRegionName; }
            set { _selectedRegionName = value;
                RaisePropertyChanged();
                FillCityes();
            }
        }

        public List<RegionOptions> Cities {
            get { return _cities; }
            set { _cities = value;
                RaisePropertyChanged();
            }
        }

        public RegionOptions SelectedRegion {
            get { return _selectedRegion; }
            set {
                _selectedRegion = value;
                RaisePropertyChanged();
                UpdateService();
                IsSelectedRegion = value!=null;
            }
        }        

        public bool IsSelectedRegion 
        {
            get { return SelectedRegion != null; }
            set { RaisePropertyChanged(); }
        }        

        private void FillCityes ()
        {
            Cities = dictRegions[SelectedRegionName];
        }

        private void UpdateService ()
        {
            if (SelectedRegion==null)
            {
                return;
            }
            if (InsService == null)
            {
                InsService = InsServiceFactory.Create(Doc, SelectedRegion);                
            }
            else
            {
                // TODO: обновление региона - ???
                throw new NotImplementedException("Обновление региона пока не реализовано.");
            }
        }
    }
}
