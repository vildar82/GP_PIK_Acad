using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.Elements.Blocks.BlockSection;

namespace PIK_GP_Acad.KP.KP_BlockSection
{
    public class BlockSectionFactory
    {
        public static BlockSectionKP CreateBS (BlockReference blRef, string blName)
        {
            BlockSectionKP bs = null;
            if (blName.Contains("Башня", StringComparison.OrdinalIgnoreCase))
            {
                bs = new TowerKPBS(blRef, blName);
            }
            else
            {
                bs = new BlockSectionKP(blRef, blName);
            }
            return bs;
        }
    }
}
