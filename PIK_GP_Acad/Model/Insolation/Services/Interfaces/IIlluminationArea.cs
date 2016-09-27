using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.Insolation.Services
{
    public interface IIlluminationArea
    {
        double AngleEndOnPlane { get; set; }
        double AngleStartOnPlane { get; set; }
        int Time { get; set; }
    }
}