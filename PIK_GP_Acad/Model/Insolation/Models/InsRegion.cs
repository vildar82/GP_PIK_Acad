using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catel.Data;
using Catel.Fody;
using Catel.IO;

namespace PIK_GP_Acad.Insolation.Models
{
    public class InsRegion : ModelBase, IEquatable<InsRegion>
    {        
        private static string FileRegions = Path.Combine(AutoCAD_PIK_Manager.Settings.PikSettings.ServerShareSettingsFolder, 
            @"ГП\Insolation\Regions.xml");
        
        public RegionEnum RegionPart { get; set; }
        public string RegionName { get; set; }
        public string City { get; set; }
        public double Latitude { get; set; }

        public InsRegion () : base() { }

        public InsRegion (RegionEnum regPart, string regName, string city, double latitude) : base()
        {
            RegionPart = regPart;
            RegionName = regName;
            City = city;
            Latitude = latitude;
        }

        public bool Equals (InsRegion other)
        {
            if (other == null) return false;
            var res = RegionPart == other.RegionPart &&
                Latitude == other.Latitude;
            return res;
        }

        public override int GetHashCode ()
        {
            return RegionPart.GetHashCode() ^ Latitude.GetHashCode();
        }
    }
}
