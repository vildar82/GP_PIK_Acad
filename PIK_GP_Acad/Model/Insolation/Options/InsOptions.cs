using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using AcadLib;
using Catel.Data;
using PIK_GP_Acad.Model.Insolation;

namespace PIK_GP_Acad.Insolation.Options
{
    public class InsOptions : ModelBase
    {
        public InsOptions () : base()
        {   
        }

        public byte Transparence { get; set; } = 120;        
        /// <summary>
        /// Размер ячейки карты - квадрата, на который разбивается вся карта
        /// </summary>
        public int TileSize { get; set; } = 1;
        public InsRegion Region { get; set; }        
        /// <summary>
        /// Шаг угла луча (в градусах) при определении теней
        /// </summary>
        public int ShadowDegreeStep { get; set; } = 1;
        /// <summary>
        /// Начальный расчетный угол (пропуская первый час). Восход = 0град.
        /// </summary>
        public double SunCalcAngleStart { get; set; } = 15.0;
        /// <summary>
        /// Конечный расчетный угол (минус последний час). Заход = 180град.
        /// </summary>
        public double SunCalcAngleEnd { get; set; } = 165.0;         
    }
}
