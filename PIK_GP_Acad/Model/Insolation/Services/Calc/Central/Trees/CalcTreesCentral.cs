using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIK_GP_Acad.Insolation.Models;

namespace PIK_GP_Acad.Insolation.Services
{
    /// <summary>
    /// Расчет Елочек в центральном регионе
    /// </summary>
    public class CalcTreesCentral : ICalcTrees
    {
        private InsCalcServiceCentral insService;

        public CalcTreesCentral (InsCalcServiceCentral centralInsService)
        {
            insService = centralInsService;
        }        

        /// <summary>
        /// Расчет инсоляции в точке
        /// </summary>
        public List<IIlluminationArea> CalcPoint (InsPoint insPoint, Map map)
        {
            List<IIlluminationArea> illumAreas;
            using (map.Doc.LockDocument())
            using (var t = map.Doc.Database.TransactionManager.StartTransaction())
            {
                var calcPt = new CalcPointCentral(insPoint, map, insService);
                // Расчет освещенности в точке
                illumAreas = calcPt.Calc();
                t.Commit();
            }
            return illumAreas;
        }
    }
}
