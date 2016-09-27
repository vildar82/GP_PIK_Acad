using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.Elements;
using PIK_GP_Acad.Insolation;
using PIK_GP_Acad.Insolation.Models;

namespace PIK_GP_Acad.Insolation.Services
{
    /// <summary>
    /// Расчет инсоляции в центральном регионе - лучевой конус это плоскость, в день равноденствия
    /// </summary>
    public class InsCalcServiceCentral : IInsCalcService
    {
        public InsCalcServiceCentral(InsOptions options)
        {
            Options = options;
            CalcValues = new CalcValuesCentral(Options);
            TreesCalc = new CalcTreesCentral(this);
        }

        public ICalcTrees TreesCalc { get; set; }
        public CalcValuesCentral CalcValues { get; set; }
        public InsOptions Options { get; set; }

        /// <summary>
        /// Определение требования освещенности
        /// </summary>
        public InsRequirement CalcTimeAndGetRate (List<IIlluminationArea> illums)
        {
            InsRequirementEnum rate = InsRequirementEnum.A;
            int maxTimeOneIlum = 0;
            int sumTimeIlum = 0;
            foreach (var item in illums)
            {
                item.Time = CalcTime(item);
                sumTimeIlum += item.Time;
                if (item.Time > maxTimeOneIlum)
                    maxTimeOneIlum = item.Time;
            }
            if (maxTimeOneIlum >= 120)
            {
                rate = InsRequirementEnum.D;
            }
            var req = InsService.GetInsReqByEnum(rate);
            return req;
        }

        private int CalcTime (IIlluminationArea item)
        {
            var angleStartOnSun = CalcValues.AngleSun(item.AngleStartOnPlane);
            var angleEndOnSun = CalcValues.AngleSun(item.AngleEndOnPlane);
            var time = Convert.ToInt32((angleEndOnSun - angleStartOnSun) * 4);// в 1 градусе 4 минуты времени хода солнца (по своей плоскости)
            return time;
        }

        /// <summary>
        /// Карта теней
        /// </summary>
        public void CreateShadowMap()
        {
            //Visual visual = new Visual();
            //visual.Show(Map);
        }             

        private void cretateIllumAreas (List<IIlluminationArea> res)
        {
            //var cs = model.Doc.Database.CurrentSpaceId.GetObject(OpenMode.ForWrite) as BlockTableRecord;
            //foreach (var illum in res)
            //{
            //    illum.Create(cs);
            //}
        }       
    }
}
