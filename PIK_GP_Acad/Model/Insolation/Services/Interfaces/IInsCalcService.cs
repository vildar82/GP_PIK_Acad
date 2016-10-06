using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.Elements.Buildings;
using PIK_GP_Acad.Insolation.Models;

namespace PIK_GP_Acad.Insolation.Services
{
    /// <summary>
    /// Сервир расчета инсоляции чертежа
    /// </summary>
    public interface IInsCalcService
    {
        InsOptions Options { get; set; }
        ICalcValues CalcValues { get; set; }
        ICalcTrees TreesCalc { get; set; }        
        void CreateShadowMap ();
        InsValue CalcTimeAndGetRate (List<IIlluminationArea> illums, BuildingTypeEnum buildingType);

        /// <summary>
        /// Определение требования инсоляции
        /// </summary>
        /// <param name="maxTimeContinuosIlum">Максимальная продолжительность непрерывного участка инсоляции</param>
        /// <param name="totalTime">Общее время инсоляции</param>
        /// <param name="buildingType">Тип здания</param>        
        InsRequirement DefineInsRequirement (int maxTimeContinuosIlum, int totalTime, BuildingTypeEnum buildingType);
    }
}