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
        public List<IIlluminationArea> CalcPoint (InsPoint insPoint, bool withOwnerBuilding = true)
        {
            List<IIlluminationArea> illumAreas = new List<IIlluminationArea>();
            
            if (withOwnerBuilding && insPoint.Building == null)
            {
                return illumAreas;
            }

            var doc = insPoint.Model.Doc;
            var calcPt = new CalcPointCentral(insPoint, insService);
            calcPt.WithOwnerBuilding = withOwnerBuilding;
            // Расчет освещенности в точке            
            illumAreas = calcPt.Calc();
            insPoint.AngleStartOnPlane = calcPt.StartAnglesIllum.AngleStartOnPlane;
            insPoint.AngleEndOnPlane = calcPt.StartAnglesIllum.AngleEndOnPlane;
            return illumAreas;
        }
    }
}
