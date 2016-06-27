using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIK_GP_Acad.Insolation.SunlightRule;

namespace PIK_GP_Acad.Insolation
{
    public abstract class Options
    {
        /// <summary>
        /// Максимальная высота расчетная
        /// </summary>
        public int MaxHeight { get; internal set; }
        public ISunlightRule SunlightRule { get; set; }

        public Options (ISunlightRule rule, int maxHeight)
        {
            SunlightRule = rule;
            MaxHeight = maxHeight;
        }
    }
}
