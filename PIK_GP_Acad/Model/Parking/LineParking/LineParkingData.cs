using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Acad.Parking
{
    public class LineParkingData
    {
        public List<LineParking> Parkings { get; set; }
        public double Places { get; set; }
        public double InvalidPlaces { get; set; }        

        public LineParkingData(List<LineParking> parkings)
        {
            Parkings = parkings;
        }   
             
        public void Calc()
        {
            Places = Parkings.Where(p => !p.IsInvalid).Sum(p => p.Places);
            InvalidPlaces = Parkings.Where(p => p.IsInvalid).Sum(p => p.Places);
        }
    }
}
