using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.XData;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using AcadLib.RTree.SpatialIndex;
using PIK_GP_Acad.Insolation.Models;

namespace PIK_GP_Acad.Elements.Buildings
{
    /// <summary>
    /// Здание
    /// </summary>
    public interface IBuilding: IElement, IDboDataSave, ITypedDataValues
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
        /// <summary>
        /// Номер этажа для расчета фронтов
        /// </summary>
        int FrontLevel { get; set; }        
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
        /// <summary>
        /// Настройки для дома, в который входит секция
        /// </summary>
        HouseOptions HouseOptions { get; set; }
        /// <summary>
        /// Здание видимо на чертеже - не изолировано, слой включен (для полилиний) и разморожен (для всех типов)
        /// </summary>
        bool IsVisible { get; set; }

        Polyline GetContourInModel();
    }
}
