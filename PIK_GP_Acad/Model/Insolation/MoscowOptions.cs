using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIK_GP_Acad.Insolation.SunlightRule;

namespace PIK_GP_Acad.Insolation
{
    public class MoscowOptions : Options
    {
        public MoscowOptions () : base(new SimpleRule())
        {
        }
    }
}
