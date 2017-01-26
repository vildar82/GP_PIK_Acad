using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.BlockSection_GP
{
    public class EstimateEkb : Estimate
    {
        public int ParkingPlace { get; set; }
        

        public EstimateEkb()
        {            
            Title = "Екатеринбург (Иркутск)";
            LiveAreaPerHuman = 30;
            KindergartenPlacePer1000 = 55;
            SchoolPlacePer1000 = 114;
            ParkingPlace = 80;
            ParkingPlaceGuestPercent = 20;
            ParkingPlacePercent = 80;
        }

        public override string GetParkingPlace()
        {            
            return $"(Sкв/{ParkingPlace})х{ParkingPlacePercent}%";
        }

        public override double GetParkingPlace(DataSection data)
        {
            return data.TotalAreaApart / ParkingPlace;
        }

        public override string GetParkingPlaceGuest()
        {
            return $"(Sкв/{ParkingPlace})х{ParkingPlaceGuestPercent}%";
        }

        public override void TableFormatting(Table table)
        {
            base.TableFormatting(table);
            //table.Cells.BackgroundColor = Color.FromColor(System.Drawing.Color.White);
            table.ColorIndex = 5;            
        }
    }
}
