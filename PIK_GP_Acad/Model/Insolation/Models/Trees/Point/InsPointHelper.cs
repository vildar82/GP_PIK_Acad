using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using AcadLib;

namespace PIK_GP_Acad.Insolation.Models
{
    public static class InsPointHelper
    {
        private const string insPointFlag = "ins";

        /// <summary>
        /// Проверка это инсоляционная точка - по записи XData Ins
        /// </summary>
        /// <param name="dbPt">Точка</param>
        /// <returns>Да/Нет</returns>
        public static bool IsInsPoint(DBPoint dbPt)
        {
            bool res = false;
            if (dbPt.XData != null)
            {
                res = dbPt.GetXDataPIK<string>() == insPointFlag;
            }
            return res;
        }

        /// <summary>
        /// Установка точки как инсоляционной - запись Xdata Ins
        /// Точка должна уже быть добавлена в базу чертежа
        /// </summary>
        /// <param name="dPt"></param>
        public static void SetInsPoint(DBPoint dPt)
        {
            dPt.SetXDataPIK(insPointFlag);
        }
    }
}
