namespace PIK_GP_Acad.Insolation.UI
{
    using Catel.MVVM;
    using System.Threading.Tasks;
    using Options;
    using System.Collections.Generic;
    using System.Linq;
    using System;
    using System.Collections.ObjectModel;

    public class InsRegionViewModel : ViewModelBase
    {
        Dictionary<string, List<InsRegion>> dictRegions = Services.InsService.Settings.Regions.ToList().
                     GroupBy(g => g.RegionName).OrderBy(o => o.Key).ToDictionary(k => k.Key, v => v.ToList());

        public InsRegionViewModel () { }
        public InsRegionViewModel (InsRegion region) : base()
        {
            InsRegion = region;
            RegionNames = dictRegions.Keys.ToList();
            SelectedRegionName = region.RegionName;
        }

        [Model]
        public InsRegion InsRegion { get; set; }

        public List<string> RegionNames { get; set; }

        public string SelectedRegionName { get; set; }

        public List<InsRegion> Cities { get; set; }        

        private void FillCityes ()
        {
            Cities = dictRegions[SelectedRegionName];
        }

        private void OnRegionNamesChanged()
        {
            FillCityes();
        }
    }
}
