using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections.ObjectModel;
using PIK_GP_Acad.Insolation.Models;

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
        public InsRegionViewModel (InsRegion region)
        {
            InsRegion = region;
            RegionNames = new ObservableCollection<string>();            
            foreach (var item in dictRegions.Keys)
            {
                RegionNames.Add(item);
            }
            SelectedRegionName = InsRegion.RegionName;
            SelectedRegion = region;
        }
        
        public InsRegion InsRegion { get; set; }
        public ObservableCollection<string> RegionNames { get; set; }
        public string SelectedRegionName { get; set; }
        public ObservableCollection<InsRegion> Cities { get; set; }
        public InsRegion SelectedRegion { get; set; }

        private void OnSelectedRegionChanged ()
        {
            if (SelectedRegion != null)
            {
                InsRegion = SelectedRegion;
                //InsRegion.RegionPart = SelectedRegion.RegionPart;
                //InsRegion.RegionName = SelectedRegion.RegionName;
                //InsRegion.City = SelectedRegion.City;
                //InsRegion.Latitude = SelectedRegion.Latitude;
            }
        }

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
    }
}
