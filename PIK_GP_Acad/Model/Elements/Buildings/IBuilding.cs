using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.XData;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using AcadLib.RTree.SpatialIndex;

namespace PIK_GP_Acad.Elements.Buildings
{
    /// <summary>
    /// Здание
    /// </summary>
    public interface IBuilding: IElement, IDboDataSave
    {
        int Floors { get; }
        Extents3d ExtentsInModel { get; }        
        int Height { get; }        
        Polyline GetContourInModel ();
        BuildingTypeEnum BuildingType { get; set; }        
        /// <summary>
        /// Имя дома - объекта
        /// </summary>
        string HouseName { get; set; }             
    }
}
