using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Acad.Insolation.SunlightRule
{
    /// <summary>
    /// Простая инсоляционная линейка
    /// </summary>
    public class SimpleRule : ISunlightRule
    {
        private double ratioLength = 1.42814;
        public int GetLength (int height)
        {
            return Convert.ToInt32(height * ratioLength);
        }
    }
}
