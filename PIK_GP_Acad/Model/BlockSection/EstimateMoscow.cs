using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Acad.BlockSection
{
    public class EstimateMoscow : Estimate
    {
        public EstimateMoscow()
        {
            Color = Autodesk.AutoCAD.Colors.Color.FromColor(System.Drawing.Color.Bisque);
            Title = "Московская область (РНГП №713/30)";
            LiveAreaPerHuman = 28;
            KindergartenPlacePer1000 = 65;
            SchoolPlacePer1000 = 135;
            ParkingPlacePer1000 = 420;
            ParkingPlaceGuestPercent = 25;
            ParkingPlacePercent = 90;
        }
    }
}
