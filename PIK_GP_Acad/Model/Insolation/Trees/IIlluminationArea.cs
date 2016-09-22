using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.Insolation
{
    public interface IIlluminationArea
    {
        double AngleEndOnPlane { get; set; }
        double AngleStartOnPlane { get; set; }
        Polyline Hight { get; set; }
        Polyline Low { get; set; }
        Polyline Medium { get; set; }

        void Create (BlockTableRecord space);
    }
}