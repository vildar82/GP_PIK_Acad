using Catel.MVVM;
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
        Dictionary<string, List<InsRegion>> dictRegions = Services.InsService.Settings.Regions.ToList().
                     GroupBy(g => g.RegionName).OrderBy(o => o.Key).ToDictionary(k => k.Key, v => v.ToList());

        public InsRegionViewModel () { }
        public InsRegionViewModel (InsRegion region) : base()
        {
            InsRegion = region;
            RegionNames = dictRegions.Keys.ToList();
            SelectedRegionName = InsRegion.RegionName;
        }

        [Model]
        public InsRegion InsRegion { get; set; }

        public List<string> RegionNames { get; set; }

        public string SelectedRegionName { get; set; }

        public List<InsRegion> Cities { get; set; }                

        private void OnRegionNamesChanged()
        {
            FillCityes();
        }
        private void FillCityes ()
        {
            if (!string.IsNullOrEmpty(SelectedRegionName))
                Cities = dictRegions[SelectedRegionName];
        }

        protected override async Task InitializeAsync ()
        {
            await base.InitializeAsync();

            // TODO: subscribe to events here  
            
        }

        protected override async Task CloseAsync ()
        {
            // TODO: unsubscribe from events here

            await base.CloseAsync();
        }
    }
}
