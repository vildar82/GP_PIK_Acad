using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.BlockSection
{
    public class EstimateMoscowRegion : Estimate
    {
        public EstimateMoscowRegion()
        {
            //Color = Autodesk.AutoCAD.Colors.Color.FromColor(System.Drawing.Color.Bisque);
            Title = "Московская область (РНГП №713/30)";
            LiveAreaPerHuman = 28;
            KindergartenPlacePer1000 = 65;
            SchoolPlacePer1000 = 135;
            ParkingPlacePer1000 = 420;
            ParkingPlaceGuestPercent = 25;
            ParkingPlacePercent = 90;
        }

        public override void TableFormatting (Table table)
        {
            base.TableFormatting(table);
            table.ColorIndex = 250;
        }
    }
}
