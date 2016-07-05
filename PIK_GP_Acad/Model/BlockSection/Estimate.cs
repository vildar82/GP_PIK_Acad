using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.BlockSection
{
    /// <summary>
    /// Расчетные показатели - расчета численности жителей
    /// </summary>
    public abstract class Estimate
    {        
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
                case "Msk":
                    return new EstimateMoscow();
                default: // Московская область
                    return new EstimateMoscowRegion();                    
            }
        }

        public virtual string GetParkingPlace()
        {
            // Для москвы
            return $"({ParkingPlacePer1000}/1000)х{ParkingPlacePercent}%";
        }

        public virtual double GetParkingPlace(DataSection data)
        {
            // Для москвы
            return  data.Population *0.001 * ParkingPlacePer1000;
        }

        public virtual string GetParkingPlaceGuest()
        {
            // Для москвы
            return $"({ParkingPlacePer1000}/1000)х{ParkingPlaceGuestPercent}%";
        }

        public virtual void TableFormatting (Table table)
        {
            //table.Cells.BackgroundColor = Color.FromColor(System.Drawing.Color.White);
            //table.Color = _service.Estimate.Color;
            table.Cells.BackgroundColor = Color.FromColor(System.Drawing.Color.White);            
        }
    }
}
