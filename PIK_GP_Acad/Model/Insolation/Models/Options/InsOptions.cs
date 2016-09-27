using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using AcadLib;
using Catel.Data;
using PIK_GP_Acad.Insolation.Services;

namespace PIK_GP_Acad.Insolation.Models
{
    public class InsOptions : ModelBase
    {
        public InsOptions ()
        {            
        }

        public byte Transparence { get; set; } = 120;        
        /// <summary>
        /// Размер ячейки карты - квадрата, на который разбивается вся карта
        /// </summary>
        public int TileSize { get; set; } = 1;
        public InsRegion Region { get; set; } = InsService.Settings.Regions[0];
        /// <summary>
        /// Шаг угла луча (в градусах) при определении теней
        /// </summary>
        public int ShadowDegreeStep { get; set; } = 1;
        /// <summary>
        /// Начальный расчетный угол (пропуская первый час) [град]. Восход(центр) = 0град + 15
        /// </summary>
        public double SunCalcAngleStart { get; set; } = 15.0;
        /// <summary>
        /// Конечный расчетный угол (минус последний час) [град]. Заход(центр) = 180град -15
        /// </summary>
        public double SunCalcAngleEnd { get; set; } = 165.0;         
    }
}
