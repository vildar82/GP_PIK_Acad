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
    /// Блок детского сада.
    /// </summary>
    public class KindergartenBlock : SocialBlock, IElement
    {
        public const string BlockName = "КП_ДОО";

        private const string ParamPlaces = "Количество мест"; // параметр видимости. Должен начинаться с числа мест

        /// <summary>
        /// Кол мест
        /// </summary>
        public int Places { get; set; }

        public KindergartenBlock (BlockReference blRef, string blName) : base(blRef, blName)
        {
            Places = GetPlaces(ParamPlaces);
        }        
    }
}
