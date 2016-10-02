using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using static AcadLib.MathExt;

namespace PIK_GP_Acad.Insolation.Services
{
    /// <summary>
    /// Угол для инсоляционных расчетов. 
    /// Начало в X=0 (как и в автокаде). 
    /// Движение по часовой стрелке (в отличае от автокада). 
    /// Отрицательные значения не допускаются. 
    /// Превышение 2Pi не допускается. 
    /// В Радианах.
    /// </summary>
    public struct InsAngleCentral
    {
        double d;

        public InsAngleCentral(double acadAngle)
        {
            d = GetInsAngleFromAcad(acadAngle);
        }

        /// <summary>
        /// Угол в радианах (по инсоляционной шкале измерений)
        /// </summary>
        public double Angle { get { return d; } }

        /// <summary>
        /// Перевод автокадовского угла (отсчет против часовой стрелки начиная с оси X направленной на Восток)
        /// в угол для инсоляции (отсчет по часовой стрелке от оси Х направленоц на восток)
        /// </summary>
        /// <param name="acadAngle">Угол полученный от автокадовских объектов чертежа (рад)</param>
        /// <returns>Угол для инсоляции (радианы)</returns>
        public static double GetInsAngleFromAcad (double acadAngle)
        {            
            return PI2 - acadAngle.FixedAngle();
        }
    }
}
