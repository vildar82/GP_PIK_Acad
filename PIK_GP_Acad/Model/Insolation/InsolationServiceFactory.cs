using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.Insolation.Central;

namespace PIK_GP_Acad.Insolation
{
    public static class InsolationServiceFactory
    {
        internal static IInsolationService Create (Document doc)
        {
            // Пока только центральный регион
            InsOptions insOpt = new InsOptions();
            IInsolationService insService = new CentralInsService(doc.Database, insOpt);
            return insService;
        }
    }
}
