using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.Insolation.Central;

namespace PIK_GP_Acad.Insolation
{
    public interface IInsolationService
    {
        Database Db { get; set; }
        Map Map { get; set; }
        InsOptions Options { get; set; }
        CalcValuesCentral CalcValues { get; set; }
        void CalcPoint (Point3d pt);
        void CreateShadowMap ();
    }
}