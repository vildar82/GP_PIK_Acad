using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Catel.MVVM;
using PIK_GP_Acad.Insolation.Options;
using PIK_GP_Acad.Insolation.UI.Trees;

namespace PIK_GP_Acad.Insolation.UI
{
    public class InsServiceViewModel : ViewModelBase
    {        
        Dictionary<string, List<RegionOptions>> dictRegions = InsOptions.Regions.
            GroupBy(g => g.RegionName).OrderBy(o => o.Key).ToDictionary(k => k.Key, v => v.ToList());                
        

        public InsServiceViewModel()
        {            
            RegionNames = dictRegions.Keys.ToList();            
        }

        public IInsolationService InsService { get; set; }

        public ObservableCollection<IInsCalcViewModel> Tabs { get; set; } = 
            new ObservableCollection<IInsCalcViewModel> { new TreesViewModel() };

        public IInsCalcViewModel SelectedTab { get; set; }

        public List<string> RegionNames { get; set; }

        public string SelectedRegionName { get; set; }

        public List<RegionOptions> Cities { get; set; }

        public RegionOptions SelectedRegion { get; set; }

        public bool IsSelectedRegion { get; set; }

        private void FillCityes ()
        {
            Cities = dictRegions[SelectedRegionName];
        }        
    }
}
