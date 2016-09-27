using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.Insolation.Models;

namespace PIK_GP_Acad.Insolation.Services
{
    /// <summary>
    /// Сервир расчета инсоляции чертежа
    /// </summary>
    public interface IInsCalcService
    {
        InsOptions Options { get; set; }

        ICalcTrees TreesCalc { get; set; }        
        void CreateShadowMap ();
        InsRequirement CalcTimeAndGetRate (List<IIlluminationArea> illums);
    }
}