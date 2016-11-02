using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.Insolation.Models;

namespace PIK_GP_Acad.Insolation.Services
{
    public static class CalcServiceFactory
    {
        public static ICalcService Create (InsOptions options)
        {
            ICalcService insService = null;
            if (options.Region.RegionPart == RegionEnum.Central)
            {
                insService = new CalcServiceCentral(options);
            }
            else
            {
                // TODO: Реализовать северный и южный регион
                throw new NotImplementedException("Расчет выбранного региона пока не реализован.");
            }
            return insService;
        }
    }
}
