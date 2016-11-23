using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.Elements.Blocks.BlockSection
{
    class TowerKPBS : BlockSectionKP
    {
        public const string BlockName = "ГП_К_Секция_Башня";

        private const string propAreaBKFN = "SБКФН";
        private const string propAreaGNS = "SГНС";
        private const string propAreaLive = "SКВ";
        
        public TowerKPBS (BlockReference blRef, string blName) : base(blRef, blName)
        {
            AreaBKFN = BlockBase.GetPropValue<double>(propAreaBKFN);
            AreaGNS = BlockBase.GetPropValue<double>(propAreaGNS);
            AreaLive = BlockBase.GetPropValue<double>(propAreaLive);
        }

        public double AreaBKFN { get; internal set; }
    }
}
