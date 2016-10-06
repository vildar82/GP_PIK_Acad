using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
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
        /// Макимальная продолжительность непрерывной инсоляции, [мин]
        /// </summary>
        public int MaxContinuosTime { get; set; }
        /// <summary>
        /// Суммарная инсоляция, [мин]
        /// </summary>
        public int TotalTime { get; set; }

        public string TotalTimeString {
            get { return TotalTime.ToHours() + "ч."; }
        }
        public string MaxContinuosTimeString {
            get { return MaxContinuosTime.ToHours() + "ч."; }
        }

    }
}
