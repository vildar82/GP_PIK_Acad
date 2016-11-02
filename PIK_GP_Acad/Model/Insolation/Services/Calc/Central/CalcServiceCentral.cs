using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.Elements;
using PIK_GP_Acad.Elements.Buildings;
using PIK_GP_Acad.Insolation;
using PIK_GP_Acad.Insolation.Models;

namespace PIK_GP_Acad.Insolation.Services
{
    /// <summary>
    /// Расчет инсоляции в центральном регионе - лучевой конус это плоскость, в день равноденствия
    /// </summary>
    public class CalcServiceCentral : ICalcService
    {
        public CalcServiceCentral(InsOptions options)
        {
            //Options = options;
            Region = options.Region;
            CalcValues = new CalcValuesCentral(options);
            CalcTrees = new CalcTreesCentral(this);
            CalcFront = new CalcFrontCentral(this);
        }

        public ICalcTrees CalcTrees { get; set; }
        public ICalcFront CalcFront { get; set; }
        public ICalcValues CalcValues { get; set; }
        //public InsOptions Options { get; set; }
        public InsRegion Region { get; set; }        

        /// <summary>
        /// Определение требования освещенности
        /// </summary>
        public InsValue CalcTimeAndGetRate (List<IIlluminationArea> illums, BuildingTypeEnum buildingType)
        {            
            int maxTimeContinuosIlum = 0;
            int curContinuosTime = 0;
            int totalTime = 0;
            IIlluminationArea prev = null;
            foreach (var item in illums)
            {
                item.Time = CalcTime(item.AngleStartOnPlane, item.AngleEndOnPlane);
                curContinuosTime += item.Time;
                totalTime += item.Time;

                if (prev != null)
                {
                    var interval = CalcTime(prev.AngleEndOnPlane, item.AngleStartOnPlane);
                    if (interval >= 10)
                    {
                        curContinuosTime = item.Time;
                    }
                }

                if (curContinuosTime > maxTimeContinuosIlum)
                    maxTimeContinuosIlum = curContinuosTime;

                prev = item;
            }

            InsRequirement req = DefineInsRequirement(maxTimeContinuosIlum, totalTime, buildingType);
            var insValue = new InsValue(req, maxTimeContinuosIlum, totalTime);
            return insValue;
        }

        public InsRequirement DefineInsRequirement (int maxTimeContinuosIlum, int totalTime, BuildingTypeEnum buildingType)
        {
            var rate = InsRequirementEnum.A;

            if (buildingType == BuildingTypeEnum.Social)
            {
                if (maxTimeContinuosIlum >= 120)
                {
                    rate = InsRequirementEnum.C;
                }
            }
            else
            {
                // Непрерывная (более 2часов)
                if (maxTimeContinuosIlum >= 120)
                {
                    rate = InsRequirementEnum.C;
                }
                else if (totalTime >= 150 && maxTimeContinuosIlum >= 60)
                {
                    rate = InsRequirementEnum.D;
                }
                else if (maxTimeContinuosIlum >= 90)
                {
                    rate = InsRequirementEnum.B;
                }
            }
            var req = InsService.GetInsReqByEnum(rate);
            return req;
        }

        private int CalcTime (double angleStart, double angleEnd)
        {
            var angleStartOnSun = CalcValues.AngleSun(angleStart);
            var angleEndOnSun = CalcValues.AngleSun(angleEnd);
            var angleDegree = (angleEndOnSun - angleStartOnSun).ToDegrees();
            var time = Convert.ToInt32(angleDegree * 4);// в 1 градусе 4 минуты времени хода солнца (по своей плоскости)
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

        public bool IsIdenticalOptions (InsOptions options)
        {
            var res = Region.Latitude == options.Region.Latitude;
            return res;            
        }
    }
}
