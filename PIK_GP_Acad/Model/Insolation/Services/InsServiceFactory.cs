using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.Insolation.Central;
using PIK_GP_Acad.Insolation.Options;

namespace PIK_GP_Acad.Insolation
{
    public static class InsServiceFactory
    {
        public static IInsolationService Create (Document doc, Options.InsRegion region)
        {
            IInsolationService insService = null;
            if (region.RegionPart == RegionEnum.Central)
            {
                insService = new CentralInsService(doc, region);
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
