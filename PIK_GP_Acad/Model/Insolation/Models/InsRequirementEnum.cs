using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Инсоляционнное требование
    /// </summary>
    public enum InsRequirementEnum
    {
        /// <summary>
        /// Прерывистая
        /// 2,5 часа прерывистой исноляции
        /// </summary>
        D,
        /// <summary>
        /// Непрерывная
        /// 2 часа непрерывной инсоляции 
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
