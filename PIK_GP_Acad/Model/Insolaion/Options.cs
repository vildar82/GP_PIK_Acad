using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIK_GP_Acad.Insolaion.SunlightRule;

namespace PIK_GP_Acad.Insolaion
{
    public class Options
    {
        /// <summary>
        /// Максимальная высота расчетная
        /// </summary>
        public int MaxHeight { get; internal set; }
        public ISunlightRule SunlightRule { get; set; }
    }
}
