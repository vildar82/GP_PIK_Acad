using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.Elements.Blocks.BlockSection
{
    /// <summary>
    /// Блок секции ПИК1 Концепции
    /// </summary>
    public class BlockSectionPIK1KP : BlockSectionKP
    {
        public const string BlockNameMatch = "ГП_ПИК1_Секция";
        public BlockSectionPIK1KP (BlockReference blRef, string blName) : base(blRef, blName)
        {
            FriendlyTypeName = "б/с ПИК1";
        }
    }
}
