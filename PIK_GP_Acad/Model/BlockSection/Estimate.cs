using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Colors;

namespace PIK_GP_Acad.BlockSection
{
    /// <summary>
    /// Расчетные показатели - расчета численности жителей
    /// </summary>
    public abstract class Estimate
    {
        public Color Color { get; set; }
        public string Title { get; set; }
        /// <summary>
        /// Жилая площадь на человека - 28м2/чел
        /// </summary>
        public double LiveAreaPerHuman { get; internal set; }
        public int KindergartenPlacePer1000 { get; internal set; }
        public int SchoolPlacePer1000 { get; internal set; }
        public int ParkingPlacePer1000 { get; internal set; }
        public int ParkingPlacePercent { get; internal set; }
        public int ParkingPlaceGuestPercent { get; internal set; }

        public static Estimate GetEstimate (string region)
        {
            switch (region)
            {                
                case "Ekb":
                    return new EstimateEkb();
                default: // Москва
                    return new EstimateMoscow();                    
            }
        }

        //public virtual string GetParkingPlace(DataSection data)
        //{
        //    // Для москвы
        //    return $"{ParkingPlacePer1000}/1000)х{ParkingPlacePercent}%)";
        //}

        //public virtual int GetParkingPlace(DataSection data)
        //{
        //    // Для москвы
        //    return 
        //}
    }
}
