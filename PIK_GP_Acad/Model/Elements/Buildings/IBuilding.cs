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
        /// Высота первого этажа (Пока, если задано, то это не жилой этаж)
        /// </summary>
        double HeightFirstFloor { get; set; }
        /// <summary>
        /// Высота типовых этажей
        /// </summary>
        double HeightTypicalFloors { get; set; }
        /// <summary>
        /// Высоте тех.этажа
        /// </summary>
        double HeightTechnicalFloor { get; set; }
        /// <summary>
        /// Относительный уровень
        /// </summary>        
        double Elevation { get; set; }
        Polyline GetContourInModel ();
        BuildingTypeEnum BuildingType { get; set; }
        string BuildingTypeName { get; }
        /// <summary>
        /// Имя дома - объекта
        /// </summary>
        string HouseName { get; set; }
        /// <summary>
        /// Связанный дом с базой MDM
        /// </summary>
        int HouseId { get; set; }
        Rectangle Rectangle { get; set; }
        /// <summary>
        /// Это проектируемое здание или нет
        /// </summary>
        bool IsProjectedBuilding { get; set; }
        /// <summary>
        /// Обиходное название типа здания - блок-секция, детсад, школа, 
        /// </summary>
        string FriendlyTypeName { get; set; }
        /// <summary>
        /// Высота до заданного этажа
        /// </summary>
        /// <param name="frontLevel">Номер этажа</param>        
        double GetLevelHeight(int frontLevel);
    }
}
