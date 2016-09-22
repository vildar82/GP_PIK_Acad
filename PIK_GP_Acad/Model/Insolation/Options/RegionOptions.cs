using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Acad.Insolation.Options
{
    public class RegionOptions
    {
        public RegionEnum RegionPart { get; set; }
        public string RegionName { get; set; }
        public string City { get; set; }
        public double Latitude { get; set; }

        public RegionOptions () { }
        public RegionOptions(RegionEnum regPart, string regName, string city, double latitude)
        {
            RegionPart = regPart;
            RegionName = regName;
            City = city;
            Latitude = latitude;
        }
    }
}
