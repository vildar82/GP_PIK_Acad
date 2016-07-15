using AcadLib;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.Elements;

namespace PIK_GP_Acad.Parkings
{
    public interface IParking : IElement
    {
        ObjectId IdBlRef { get; set; }
        int InvalidPlaces { get; set; }
        int Places { get; set; }
        void Calc();        
    }
}