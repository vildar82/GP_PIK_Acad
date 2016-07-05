using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Acad.KP.Parking.Area
{
    /// <summary>
    /// Типы парковок
    /// </summary>
    public class ParkingType
    {
        public static readonly ParkingType ParkingBuildingUnderground = new ParkingType("Подземная", 40, true);
        public static readonly ParkingType ParkingBuilding = new ParkingType("Многоуровневая наземная", 35, true);
        public static readonly ParkingType ParkingArea = new ParkingType("Плоскостная наземная", 23, false);

        public string Name { get; set; }
        public double PlaceArea { get; set; }
        public bool IsBuilding { get; set; }

        private ParkingType(string name, double placeArea, bool isBuilding)
        {
            Name = name;
            PlaceArea = placeArea;
            IsBuilding = isBuilding;
        }

        public override string ToString ()
        {
            return Name;
        }
    }
}
