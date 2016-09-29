using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.Insolation.Models;

namespace PIK_GP_Acad.Insolation.Services
{
    public interface IIlluminationArea: IVisual
    {
        InsPoint InsPoint { get; set; }
        Point2d PtOrig { get; set; }
        Point2d PtStart { get; set; }
        Point2d PtEnd { get; set; }
        double AngleEndOnPlane { get; set; }
        double AngleStartOnPlane { get; set; }
        int Time { get; set; }
    }
}