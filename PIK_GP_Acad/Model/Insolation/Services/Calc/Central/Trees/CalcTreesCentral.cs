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
        private CalcServiceCentral insService;

        public CalcTreesCentral (CalcServiceCentral centralInsService)
        {
            insService = centralInsService;
        }        

        /// <summary>
        /// Расчет инсоляции в точке
        /// </summary>
        public List<IIlluminationArea> CalcPoint (InsPoint insPoint)
        {
            List<IIlluminationArea> illumAreas = new List<IIlluminationArea> ();

            if (insPoint.Building == null)
            {
                return illumAreas;
            }

            var doc = insPoint.Model.Doc;
            //using (doc.LockDocument())
            //using (var t = doc.Database.TransactionManager.StartTransaction())
            //{
                var calcPt = new CalcPointCentral(insPoint, insService);
                // Расчет освещенности в точке
                illumAreas = calcPt.Calc();
                insPoint.AngleStartOnPlane = calcPt.AngleStartOnPlane;
                insPoint.AngleEndOnPlane = calcPt.AngleEndOnPlane;
                //t.Commit();
            //}
            return illumAreas;
        }
    }
}
