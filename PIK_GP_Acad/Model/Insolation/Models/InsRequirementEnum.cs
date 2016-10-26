using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        [Description("Не определено")]        
        None,
        /// <summary>
        /// Прерывистая
        /// 2,5 часа прерывистой исноляции (при этом непрерывная >=1ч)
        /// </summary>
        [Description("Прерывистая")]        
        D,
        /// <summary>
        /// Непрерывная
        /// 2 часа непрерывной инсоляции 
        /// </summary>
        [Description("Непрерывная")]        
        C,
        /// <summary>
        /// не больше 1,5 часа инсоляции (непрерывной)
        /// </summary>        
        [Description("Промежуточная")]        
        B,
        /// <summary>
        /// Недостаточная инсоляция
        /// </summary>
        [Description("Недостаточная")]        
        A               
    }
}
