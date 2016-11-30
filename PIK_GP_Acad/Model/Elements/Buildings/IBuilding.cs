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
        int Floors { get; set; }
        Extents3d ExtentsInModel { get; set; }        
        double Height { get; set; }                
        /// <summary>
        /// Относительный уровень
        /// </summary>
        double Elevation { get; set; }
        Polyline GetContourInModel ();
        BuildingTypeEnum BuildingType { get; set; }        
        /// <summary>
        /// Имя дома - объекта
        /// </summary>
        string HouseName { get; set; }
        /// <summary>
        /// Связанный дом с базой MDM
        /// </summary>
        int HouseId { get; set; }
        Rectangle Rectangle { get; set; }
    }
}
