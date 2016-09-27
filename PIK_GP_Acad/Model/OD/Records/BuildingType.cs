using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Acad.OD.Records
{
    public class BuildingType
    {
        public static readonly BuildingType Live = new BuildingType(ODBuilding.BuildingLive);
        public static readonly BuildingType Social = new BuildingType(ODBuilding.BuildingSocial);
        public static readonly BuildingType Garage = new BuildingType(ODBuilding.BuildingGarage);

        public string Name { get; private set; }

        private BuildingType (string name)
        {
            Name = name;
        }
    }
}
