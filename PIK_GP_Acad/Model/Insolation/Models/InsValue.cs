using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using Catel.Data;
using Catel.Fody;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Значение инсоляции
    /// </summary>
    public class InsValue : ModelBase
    {
        public InsValue () { }
        /// <summary>
        /// Значение инсоляции
        /// </summary>
        /// <param name="req">Требование</param>
        /// <param name="maxTime">Мак продолжительность, мин</param>
        /// <param name="totalTime">Общая продолжительночть, мин</param>
        public InsValue (InsRequirement req, int maxTime, int totalTime)
        {
            Requirement = req;
            MaxContinuosTime = maxTime;
            TotalTime = totalTime;
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
            get { return TotalTime.ToHours(); }
        }
        public string MaxContinuosTimeString {
            get { return MaxContinuosTime.ToHours(); }
        }

        private static InsValue empty;
        /// <summary>
        /// Пустой расчет точки - когда не определено здание для точки
        /// Не изменять этот объект!!!
        /// </summary>
        [NoWeaving]
        public static InsValue Empty {
            get {
                if (empty == null)
                {
                    empty = new InsValue(new InsRequirement() { Color = System.Drawing.Color.Gray }, 0, 0);
                }
                return empty;
            }
        }
    }
}
