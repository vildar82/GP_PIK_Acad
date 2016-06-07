using AcadLib;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.Parkings
{
    public interface IParking
    {
        ObjectId IdBlRef { get; set; }
        bool IsInvalid { get; set; }
        double Places { get; set; }

        void Calc();
        Result Define(BlockReference blRef);
    }
}