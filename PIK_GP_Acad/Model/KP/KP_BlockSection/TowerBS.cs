using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.KP.KP_BlockSection
{
    class TowerBS : BlockSection
    {
        private const string propAreaBKFN = "SБКФН";
        private const string propAreaGNS = "SГНС";
        private const string propAreaLive = "SКВ";

        public double AreaBKFN { get; internal set; }
        public TowerBS (BlockReference blRef, string blName) : base(blRef, blName)
        {
            Define(blRef);
        }

        protected override void Define (BlockReference blRef)
        {
            base.Define(blRef);
            AreaBKFN = GetPropValue<double>(propAreaBKFN);
            AreaGNS = GetPropValue<double>(propAreaGNS);
            AreaLive = GetPropValue<double>(propAreaLive);
        }
    }
}
