using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Acad.BlockSection
{
    public class EstimateEkb : Estimate
    {
        public EstimateEkb()
        {
            Color = Autodesk.AutoCAD.Colors.Color.FromColor(System.Drawing.Color.Chocolate);
            Title = "Екатеринбург (Иркутск)";
            LiveAreaPerHuman = 30;
            KindergartenPlacePer1000 = 55;
            SchoolPlacePer1000 = 114;
            ParkingPlacePer1000 = 80;
            ParkingPlaceGuestPercent = 20;
            ParkingPlacePercent = 80;
        }

        //public override string GetParkingPlace(DataSection data)
        //{
        //    return $"{data.TotalArea}/{ParkingPlacePer1000})х{ParkingPlacePercent}%)";
        //}
    }
}
