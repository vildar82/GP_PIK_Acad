using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catel.Data;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Настройки расчетной точки
    /// </summary>
    public class WindowOptions : ModelBase
    {
        public WindowOptions ()
        {
        }

        /// <summary>
        /// Ширина окна, м
        /// </summary>
        public double Width { get; set; } = 1.5;
        /// <summary>
        /// Тип конструкции
        /// </summary>
        public WindowConstruction Construction { get; set; } = WindowConstruction.WindowConstructions[0];
        /// <summary>
        /// Глубина четверти
        /// </summary>
        public double Quarter { get; set; } = 0.07;        

        /// <summary>
        /// Теневой угол, рад
        /// </summary>
        /// <returns></returns>
        public double GetShadowAngle()
        {
            double b = Math.Atan2(Construction.Depth + Quarter, Width + 0.065);
            return b;
        }
    }
}
