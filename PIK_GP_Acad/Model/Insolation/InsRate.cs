using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Acad.Model.Insolation
{
    /// <summary>
    /// Инсоляционнное требование
    /// </summary>
    public enum InsRate
    {
        /// <summary>
        /// 2,5 часа прерывистой исноляции
        /// </summary>
        D,
        /// <summary>
        /// 2 часа инсоляции
        /// </summary>
        C,
        /// <summary>
        /// не больше 1,5 часа инсоляции
        /// </summary>
        B,        
        /// <summary>
        /// Не достаточная инсоляция
        /// </summary>
        A               
    }
}
