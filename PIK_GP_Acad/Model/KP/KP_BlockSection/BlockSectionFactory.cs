using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.KP.KP_BlockSection
{
    public class BlockSectionFactory
    {
        public static BlockSection CreateBS (BlockReference blRef, string blName)
        {
            BlockSection bs = null;
            if (blName.Contains("Башня", StringComparison.OrdinalIgnoreCase))
            {
                bs = new TowerBS(blRef, blName);
            }
            else
            {
                bs = new BlockSection(blRef, blName);
            }
            return bs;
        }
    }
}
