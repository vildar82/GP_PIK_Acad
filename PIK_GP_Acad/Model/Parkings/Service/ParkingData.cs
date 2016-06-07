using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIK_GP_Acad.Parkings;

namespace PIK_GP_Acad.Parkings
{
    public class ParkingData
    {
        public List<IParking> Parkings { get; set; }
        /// <summary>
        /// Кол машиномест
        /// </summary>
        public double Places { get; set; }
        /// <summary>
        /// В том числе инвалидских мест
        /// </summary>
        public double InvalidPlaces { get; set; }        

        public ParkingData(List<IParking> parkings)
        {
            Parkings = parkings;
        }   
             
        public void Calc()
        {
            Places = Parkings.Sum(p => p.Places);
            InvalidPlaces = Parkings.Where(p => p.IsInvalid).Sum(p => p.Places);
        }
    }
}
