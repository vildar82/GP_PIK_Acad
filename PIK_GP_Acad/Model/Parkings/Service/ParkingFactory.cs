using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.Parkings
{
    public static class ParkingFactory
    {
        public static IParking CreateParking(BlockReference blRef, string blName)
        {
            IParking parking = null;
            switch (blName)
            {
                case LineParking.LineParkingBlockName:
                    parking = new LineParking();
                    break;
                case Parking.ParkingBlockName:
                    parking = new Parking();
                    break;
                default:
                    break;
            }
            return parking;
        }
    }
}
