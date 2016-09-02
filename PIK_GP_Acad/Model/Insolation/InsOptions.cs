using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Colors;
using PIK_GP_Acad.Model.Insolation;

namespace PIK_GP_Acad.Insolation
{
    public class InsOptions
    {
        public byte Transparence { get; set; } = 120;
        public Color ColorLow { get; set; } = Color.FromRgb(205,32,39);
        public Color ColorMedium { get; set; } = Color.FromRgb(241,235, 31);
        public Color ColorHight { get; set; } = Color.FromRgb(19, 155, 72);
        public int LowHeight { get; set; } = 35;
        public int MediumHeight { get; set; } = 55;
        public int MaxHeight { get; set; } = 75;                
        /// <summary>
        /// Размер ячейки карты - квадрата, на который разбивается вся карта
        /// </summary>
        public int TileSize { get; set; } = 1;
        public Region Region { get; set; }
        /// <summary>
        /// Широта
        /// </summary>
        public double Latitude { get; set; }
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

        public InsOptions ()
        {            
        }
    }
}
