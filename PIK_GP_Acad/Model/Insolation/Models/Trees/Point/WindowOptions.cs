using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catel.Data;
using Catel.Runtime.Serialization;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Настройки расчетной точки
    /// </summary>
    [Serializable]
    public class WindowOptions : ModelBase
    {
        public WindowOptions (double width, double quarter, bool isCustomAngle,
            double shadowAngle, WindowConstruction constr)
        {
            // ??? Отключить события Property Changed на время инициализации свойств. Сейчас, при изменении любого свойства срабатывает событие OnPropertyChanged
            Width = width == 0? 1.5: width;
            Quarter = quarter == 0? 0.07:quarter;
            IsCustomAngle = isCustomAngle;
            ShadowAngle = shadowAngle;
            Construction = constr?? WindowConstruction.WindowConstructions[0];              
        }

        /// <summary>
        /// Ширина окна, м
        /// </summary>
        public double Width { get; set; }
        /// <summary>
        /// Тип конструкции
        /// </summary>
        [IncludeInSerialization]
        public WindowConstruction Construction { get; set; }
        /// <summary>
        /// Глубина четверти
        /// </summary>
        public double Quarter { get; set; } 
        /// <summary>
        /// Угол который ограничивает инсоляцию при расчете точки (симметрично с двух сотор) 
        /// [рад]
        /// </summary>
        public double ShadowAngle { get; set; }    
        /// <summary>
        /// Если значение угла введено пользователем
        /// </summary>
        public bool IsCustomAngle { get; set; }              

        /// <summary>
        /// Определение теневого угла - по введенным значениям
        /// </summary>        
        public double CalcShadowAngle()
        {
            if (Construction == null) return 0;                    
            double b = Math.Atan2(Construction.Depth + Quarter, Width + 0.065);
            return b;
        }

        public static WindowOptions Default()
        {
            return new WindowOptions(1.5, 0.07, false, 0, WindowConstruction.WindowConstructions[0]);
        }

        protected override void OnPropertyChanged (AdvancedPropertyChangedEventArgs e)
        {            
            if (!IsCustomAngle && e.PropertyName != nameof(ShadowAngle))
            {
                ShadowAngle = CalcShadowAngle();
            }
            base.OnPropertyChanged(e);
        }
    }
}
