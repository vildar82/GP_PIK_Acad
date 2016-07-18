using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.Elements.Blocks.Social
{
    /// <summary>
    /// Блок школы.
    /// </summary>
    public class SchoolBlock : SocialBuilding
    {
        public const string BlockName = "КП_СОШ";
        const string contourLayer = "_ГП_здания СОШ";

        public override ObjectId IdPlContour { get; set; }    

        public SchoolBlock (BlockReference blRef, string blName) : base(blRef, blName, contourLayer)
        {
            
        }        
    }
}
