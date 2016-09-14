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
        private double ratioYtoC;

        /// <summary>
        /// Угол Фи в радианах - широта
        /// </summary>
        public double Fi { get; private set; }
        /// <summary>
        /// Стартовый расчетный угол (15град для центрального региона) - в радианах.
        /// Одинаковые для всех расчетов в заданном регионе
        /// </summary>
        public double SunCalcAngleStart { get; private set; }        
        public double SunCalcAngleStartOnPlane { get; private set; }
        /// <summary>
        /// Конечый расчетный угол (165град для центрального региона) - в радианах.
        /// </summary>
        public double SunCalcAngleEnd { get; private set; }
        public double SunCalcAngleEndOnPlane { get; private set; }

        public CalcValuesCentral(InsOptions options)
        {
            Fi = options.Latitude.ToRadians();
            FiTan = Math.Tan(Fi);
            FiCos = Math.Cos(Fi);
            double cShadowNormal;
            var yShasowNormal = YShadowLineByHeight(1, out cShadowNormal);
            ratioYtoC = yShasowNormal / cShadowNormal;

            SunCalcAngleStart = options.SunCalcAngleStart.ToRadians();
            SunCalcAngleStartOnPlane = AngleSunOnPlane(SunCalcAngleStart);
            SunCalcAngleEnd = options.SunCalcAngleEnd.ToRadians();
            SunCalcAngleEndOnPlane = AngleSunOnPlane(SunCalcAngleEnd);
        }

        /// <summary>
        /// Длина до тени
        /// </summary>
        /// <param name="height">Высота</param>
        /// <param name="cShadowPlane">Гипотенуза на плоскости солнца</param>
        /// <returns>Длина тени на земле (по перпендикуляру)</returns>
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

        /// <summary>
        /// Угол проекции данного угла солнца
        /// </summary>
        /// <param name="angleSun">Угол солнца (от 0 до Пи)</param>
        /// <returns>Угол проекции на землю</returns>
        public double AngleSunOnPlane (double angleSun)
        {
            var c = Math.Tan(angleSun);
            var y = c * ratioYtoC;
            var resAngleOnPlane = Math.Atan(y);
            if (resAngleOnPlane < 0)
                resAngleOnPlane = Math.PI + resAngleOnPlane;
            return resAngleOnPlane;
        }

        /// <summary>
        /// Отклонение луча в сторону - длина и направление отклонения
        /// +- по X, откладывать от расчетной точки
        /// </summary>
        /// <param name="cSunPlane">Кактет противоположный углу луча солнца (или yShadow или cShadow)</param>
        /// <param name="angleSun">Угол луча солнца от 0 до Пи</param>        
        public double GetXRay (double cSunPlane, double angleSun)
        {   
            var res = cSunPlane / Math.Tan(angleSun);
            if (angleSun > Math.PI)
            {
                res = -res;
            }
            return res;
        }
    }
}
