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

        public double AreaFirstExternalWalls { get; private set; }
        public double AreaFirstLive { get; private set; }
        public double AreaUpperExternalWalls { get; private set; }
        public double AreaUpperLive { get; private set; }
        public double AreaTotalExternalWalls { get; private set; }
        public double AreaTotalLive { get; private set; }

        public DataSection(List<BlockSection> blocks)
        {
            this.blocks = blocks;
        }

        internal void Calc()
        {
            foreach (var blSec in blocks)
            {
                AreaFirstExternalWalls += blSec.AreaByExternalWalls;
                AreaUpperExternalWalls += blSec.AreaByExternalWalls * (blSec.Floors - 1);
            }
            AreaFirstLive = AreaFirstExternalWalls * 0.68;
            AreaUpperLive = AreaUpperExternalWalls * 0.68;

            AreaTotalExternalWalls = AreaFirstExternalWalls + AreaUpperExternalWalls;
            AreaTotalLive = AreaFirstLive + AreaUpperLive;            
        }
    }
}
