using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catel.Data;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Значение инсоляции
    /// </summary>
    public class InsValue : ModelBase
    {   
        public InsValue ()
        {

        }

        /// <summary>
        /// Инсоляционнное требование соответствующее данному значению инсоляции
        /// </summary>
        public InsRequirement Requirement { get; set; }
        /// <summary>
        /// Макимальная продолжительность непрерывной инсоляции, [ч.]
        /// </summary>
        public double MaxContinuosTime { get; set; }
        /// <summary>
        /// Суммарная инсоляция, [ч.]
        /// </summary>
        public double TotalTime { get; set; }
    }
}
