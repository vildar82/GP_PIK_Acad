using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;

namespace PIK_GP_Acad.Insolation.Central
{
    public class CalcValuesCentral
    {
        private double FiTan;
        private double FiCos;

        /// <summary>
        /// Угол Фи в радианах - широта
        /// </summary>
        public double Fi { get; private set; }
        /// <summary>
        /// Стартовый расчетный угол (15град для центрального региона) - в радианах.
        /// </summary>
        public double SunCalcAngleStart { get; private set; }
        /// <summary>
        /// Конечый расчетный угол (165град для центрального региона) - в радианах.
        /// </summary>
        public double SunCalcAngleEnd { get; private set; }

        public CalcValuesCentral(InsOptions options)
        {
            Fi = options.Latitude.ToRadians();
            FiTan = Math.Tan(Fi);
            FiCos = Math.Cos(Fi);
            SunCalcAngleStart = options.SunCalcAngleStart.ToRadians();
            SunCalcAngleEnd = options.SunCalcAngleEnd.ToRadians();
        }

        public double YShadowLineByHeight (int height, out double cShadowPlane)
        {
            var res = height * FiTan;
            cShadowPlane = height / FiCos;
            return res;
        }

        /// <summary>
        /// Определение угла в плане по двум катетам (высоте тени и отступу от вертикали до точки)
        /// </summary>
        /// <param name="yShadow">Высота тени - положительное</param>
        /// <param name="xShadow">Отступ по вертикали - с восхода положительное число, с запада - отрицательное</param>
        /// <returns>Угол в радианах</returns>
        public double AngleSunOnPlane (double yShadow,double xShadow)
        {
            var res = Math.Atan2(xShadow, yShadow);
            return res;
        }
    }
}
