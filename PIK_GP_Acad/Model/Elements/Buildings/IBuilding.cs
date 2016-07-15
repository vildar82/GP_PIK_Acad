using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace PIK_GP_Acad.Elements.Buildings
{
    /// <summary>
    /// Здание
    /// </summary>
    public interface IBuilding
    {
        int Floors { get; }
        Extents3d ExtentsInModel { get; }
        Polyline Contour { get; }
        int Height { get; set; }

        //Polyline GetContour ();
    }
}
