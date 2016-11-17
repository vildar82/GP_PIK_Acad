using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using PIK_GP_Acad.Insolation;
using PIK_GP_Acad.Insolation.Models;

namespace PIK_GP_Acad.Insolation.Services
{
    public class CalcValuesCentral : ICalcValues
    {
        private Dictionary<double, double> dictAngleFromAcad = new Dictionary<double, double>();
        private Dictionary<Tuple<double, double>, double> dictXRay = new Dictionary<Tuple<double, double>, double>();
        private Dictionary<double, Tuple<double, double>> dictYShadowLineByHeight = new Dictionary<double, Tuple<double, double>>();
        private Dictionary<double, double> dictAngleSun = new Dictionary<double, double>();
        private double FiTan;
        private double FiCos;
        /// <summary>
        /// Отношение перпендикуляров солнца на проекции и на плоскости солнца (<1)
        /// </summary>
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
            Fi = options.Region.Latitude.ToRadians();
            FiTan = Math.Tan(Fi);
            FiCos = Math.Cos(Fi);
            double cShadowNormal;
            var yShadowNormal = YShadowLineByHeight(1, out cShadowNormal);
            ratioYtoC = yShadowNormal / cShadowNormal;

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
        public double YShadowLineByHeight (double height, out double cShadowPlane)
        {
            Tuple<double, double> val;
            if (!dictYShadowLineByHeight.TryGetValue(height, out val))
            {
                var y = height * FiTan;
                cShadowPlane = height / FiCos;
                val = new Tuple<double, double>(y, cShadowPlane);
                dictYShadowLineByHeight.Add(height, val);
            }
            else
            {
                cShadowPlane = val.Item2;
            }
            return val.Item1;
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
        /// Угол солнца по углу проекции
        /// </summary>
        /// <param name="angleOnPlane">Угол на проекции (рад)</param>        
        public double AngleSun (double angleOnPlane)
        {            
            double resAngleSun;
            if (!dictAngleSun.TryGetValue(angleOnPlane, out resAngleSun))
            {
                var y = Math.Tan(angleOnPlane);
                var c = y / ratioYtoC;
                resAngleSun = Math.Atan(c);
                if (resAngleSun < 0)
                    resAngleSun = Math.PI + resAngleSun;
                dictAngleSun.Add(angleOnPlane, resAngleSun);
            }
            return resAngleSun;
        }

        /// <summary>
        /// Угол проекции данного угла солнца
        /// </summary>
        /// <param name="angleSun">Угол солнца (от 0 до Пи)</param>
        /// <returns>Угол проекции на землю</returns>
        public double AngleSunOnPlane (double angleSun)
        {
            Dictionary<double, double> dictAngleSunOnPlane = new Dictionary<double, double>();
            double resAngleOnPlane;
            if (!dictAngleSunOnPlane.TryGetValue(angleSun, out resAngleOnPlane))
            {
                var c = Math.Tan(angleSun);
                var y = c * ratioYtoC;
                resAngleOnPlane = Math.Atan(y);
                if (resAngleOnPlane < 0)
                    resAngleOnPlane = Math.PI + resAngleOnPlane;

                dictAngleSunOnPlane.Add(angleSun, resAngleOnPlane);
            }
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
            double res;
            var key = new Tuple<double, double>(cSunPlane, angleSun);
            if (!dictXRay.TryGetValue(key, out res))
            {
                res = cSunPlane / Math.Tan(angleSun);
                if (angleSun > Math.PI)
                {
                    res = -res;
                }
                dictXRay.Add(key, res);
            }
            return res;
        }

        /// <summary>
        /// Перевод автокадовского угла (отсчет против часовой счтрелки начиная с оси X направленной на Восток)
        /// в угол для инсоляции (отсчет по часовой стрелке от оси Х направленоц на восток)
        /// </summary>
        /// <param name="acadAngle">Угол полученный от автокадовских объектов чертежа (рад)</param>
        /// <returns>Угол для инсоляции (радианы)</returns>
        public double GetInsAngleFromAcad (double acadAngle)
        {
            double res;
            if (!dictAngleFromAcad.TryGetValue(acadAngle, out res))
            {
                res = acadAngle.FixedAngle();
                if (res > 0.01)
                    res = MathExt.PI2 - res;
                dictAngleFromAcad.Add(acadAngle, res);
            }
            return res;
        }
    }
}
