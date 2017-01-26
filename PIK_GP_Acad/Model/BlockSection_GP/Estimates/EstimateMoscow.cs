using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.BlockSection_GP
{
    public class EstimateMoscow : Estimate
    {
        public EstimateMoscow()
        {
            //Color = Autodesk.AutoCAD.Colors.Color.FromColor(System.Drawing.Color.Bisque);
            Title = "Москва (Временный порядок)";
            LiveAreaPerHuman = 40;
            KindergartenPlacePer1000 = 54;
            SchoolPlacePer1000 = 124;
            ParkingPlacePer1000 = 350;
            ParkingPlaceGuestPercent = 25;
            ParkingPlacePercent = 90;
        }

        public override void TableFormatting (Table table)
        {
            base.TableFormatting(table);
            //table.Cells.BackgroundColor = Color.FromColor(System.Drawing.Color.White);
            table.ColorIndex = 1;
        }

        public override double CalcPopulation (DataSection data)
        {
            return Math.Floor(data.KP_GNS_Total / LiveAreaPerHuman);
        }
    }
}
