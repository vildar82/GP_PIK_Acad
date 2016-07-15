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
    public class SchoolBlock : SocialBlock, IElement
    {
        public const string BlockName = "КП_СОШ";

        private const string ParamPlaces = "^Количество мест"; // параметр видимости. Должен начинаться с числа мест

        /// <summary>
        /// Кол мест
        /// </summary>
        public int Places { get; set; }

        public SchoolBlock (BlockReference blRef, string blName) : base(blRef, blName)
        {
            Places = GetPlaces(ParamPlaces);
        }        
    }
}
