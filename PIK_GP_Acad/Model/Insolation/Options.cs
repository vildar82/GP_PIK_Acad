using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Colors;
using PIK_GP_Acad.Insolation.SunlightRule;

namespace PIK_GP_Acad.Insolation
{
    public abstract class Options
    {
        public byte Transparence { get; set; } = 120;
        public Color LowHeightColor { get; set; } = Color.FromRgb(205,32,39);
        public Color MediumHeightColor { get; set; } = Color.FromRgb(241,235, 31);
        public Color MaxHeightColor { get; set; } = Color.FromRgb(19, 155, 72);
        public int LowHeight { get; set; } = 35;
        public int MediumHeight { get; set; } = 55;
        /// <summary>
        /// Максимальная высота расчетная
        /// </summary>
        public int MaxHeight { get; set; } = 75;
        public ISunlightRule SunlightRule { get; set; }
        /// <summary>
        /// Шаг угла сканирования
        /// </summary>
        public double ScaningStepLarge { get; set; } = 1;
        /// <summary>
        /// Шаг угла сканирования - уточняющий
        /// </summary>
        public double ScaningStepSmall { get; set; } = 0.1;
        /// <summary>
        /// Размер ячейки карты - квадрата, на который разбивается вся карта
        /// </summary>
        public int TileSize { get; set; } = 1;

        public Options (ISunlightRule rule)
        {
            SunlightRule = rule;            
        }
    }
}
