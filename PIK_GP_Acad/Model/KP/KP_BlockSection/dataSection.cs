using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Acad.KP.KP_BlockSection
{
    /// <summary>
    /// Данные для заполнения в таблицу
    /// </summary>
    class DataSection
    {
        private List<BlockSection> blocks;
        private Options options;

        public double AreaFirstGNS { get; private set; }
        public double AreaFirstLive { get; private set; }
        public double AreaUpperGNS { get; private set; }
        public double AreaUpperLive { get; private set; }
        public double AreaTotalGNS { get; private set; }
        public double AreaTotalLive { get; private set; }

        /// <summary>
        /// Населегние, чел.
        /// </summary>
        public int Population { get; private set; }
        /// <summary>
        /// СОШ мест
        /// </summary>
        public int SchoolPlaces { get; private set; }
        /// <summary>
        /// ДОО мест
        /// </summary>
        public int KinderPlaces { get; private set; }

        /// <summary>
        /// Постоянный паркинг м/м
        /// </summary>
        public int PersistentParking { get; private set; }
        /// <summary>
        /// Временный паркинг м/м
        /// </summary>
        public int TemproraryParking { get; private set; }
        /// <summary>
        /// Паркинг для БКФН
        /// </summary>
        public int ParkingBKFN { get; private set; }

        public DataSection(List<BlockSection> blocks, Options options)
        {
            this.blocks = blocks;
            this.options = options;
        }

        public void Calc ()
        {
            // Расчет площадей обычных блок-секций - без башень
            foreach (var blSec in blocks.Where(b => !(b is TowerBS)))
            {
                AreaFirstGNS += blSec.AreaGNS;
                AreaUpperGNS += blSec.AreaGNS * (blSec.Floors - 1);
                AreaFirstLive += (blSec.AreaGNS - 77) * 0.68;
            }
            AreaUpperLive = AreaUpperGNS * 0.68;

            // Расчет башен    
            foreach (var tower in blocks.OfType<TowerBS>())
            {
                AreaFirstGNS += tower.AreaGNS;
                AreaUpperGNS += tower.AreaGNS * (tower.Floors - 1);
                AreaFirstLive += tower.AreaBKFN;
                AreaUpperLive += tower.AreaLive * (tower.Floors - 1);
            }

            AreaTotalGNS = AreaFirstGNS + AreaUpperGNS;
            AreaTotalLive = AreaFirstLive + AreaUpperLive;

            Population = Convert.ToInt32(AreaUpperLive / options.NormAreaPerPerson);
            SchoolPlaces = Convert.ToInt32(Population * options.NormSchoolPlace * 0.001);
            KinderPlaces = Convert.ToInt32(Population * options.NormKinderPlace * 0.001);
            PersistentParking = Convert.ToInt32(Population * options.NormParking * 0.9 * 0.001);
            TemproraryParking = Convert.ToInt32(Population * options.NormParking * 0.25 * 0.001);
            ParkingBKFN = Convert.ToInt32((AreaFirstLive/options.NormAreaBKFNPerPerson)*0.01 * options.NormParkingPlaceFor100);
        }                
    }
}
