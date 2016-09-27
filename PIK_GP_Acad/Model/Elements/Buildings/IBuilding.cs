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
    public interface IBuilding: IElement
    {
        int Floors { get; }
        Extents3d ExtentsInModel { get; }        
        int Height { get; }        
        Polyline GetContourInModel ();
        BuildingTypeEnum BuildingType { get; set; }
    }
}
