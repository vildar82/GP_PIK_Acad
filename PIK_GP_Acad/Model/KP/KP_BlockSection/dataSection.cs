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

        public double AreaFirstExternalWalls { get; private set; }
        public double AreaFirstLive { get; private set; }
        public double AreaUpperExternalWalls { get; private set; }
        public double AreaUpperLive { get; private set; }
        public double AreaTotalExternalWalls { get; private set; }
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
            foreach (var blSec in blocks)
            {
                AreaFirstExternalWalls += blSec.AreaByExternalWalls;
                AreaUpperExternalWalls += blSec.AreaByExternalWalls * (blSec.Floors - 1);

                if (blSec.Floors <= 18)
                {
                    AreaFirstLive += (blSec.AreaByExternalWalls - 70) * 0.68;                    
                }
                else
                {
                    AreaFirstLive += (blSec.AreaByExternalWalls - 77) * 0.68;
                }
            }
            
            AreaUpperLive = AreaUpperExternalWalls * 0.68;

            AreaTotalExternalWalls = AreaFirstExternalWalls + AreaUpperExternalWalls;
            AreaTotalLive = AreaFirstLive + AreaUpperLive;

            Population = Convert.ToInt32(AreaUpperExternalWalls / options.NormAreaPerPerson);
            SchoolPlaces = Convert.ToInt32(Population * options.NormSchoolPlace * 0.001);
            KinderPlaces = Convert.ToInt32(Population * options.NormKinderPlace * 0.001);
            PersistentParking = Convert.ToInt32(Population * options.NormParking * 0.001);
            TemproraryParking = Convert.ToInt32(PersistentParking * 0.25);
            ParkingBKFN = Convert.ToInt32((AreaFirstLive/options.NormParkingAreaPerPerson)*0.01 * options.NormParkingPlaceFor100);
        }                
    }
}
