using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.Insolation.Central;
using PIK_GP_Acad.Insolation.Options;

namespace PIK_GP_Acad.Insolation
{
    /// <summary>
    /// Сервир расчета инсоляции чертежа
    /// </summary>
    public interface IInsolationService
    {
        Database Db { get; set; }
        Map Map { get; set; }
        InsOptions Options { get; set; }
        CalcValuesCentral CalcValues { get; set; }
        IInsTreeService Trees { get; set; }

        void CalcPoint (Point3d pt);
        void CreateShadowMap ();
    }
}