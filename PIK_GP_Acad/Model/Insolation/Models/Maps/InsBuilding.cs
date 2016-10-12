using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Catel.Data;
using Catel.Runtime.Serialization;
using PIK_GP_Acad.Elements.Buildings;
using PIK_GP_Acad.Insolation.Services;

namespace PIK_GP_Acad.Insolation.Models
{
    public class InsBuilding
    {
        [ExcludeFromSerialization]
        public IBuilding Building { get; private  set; }
        [ExcludeFromSerialization]
        public Polyline Contour { get; private set; }
        public int Height { get; private set; }
        public double YMax { get; private set; }
        public double YMin { get; private set; }
        [ExcludeFromSerialization]
        public Extents3d ExtentsInModel { get; private set; }
        public BuildingTypeEnum BuildingType { get; set; }
        [ExcludeFromSerialization]
        public string BuildinTypeName { get { return InsService.GetDisplayName(BuildingType); } }

        public InsBuilding () { }

        public InsBuilding(IBuilding building)
        {
            Building = building;            
            Height = building.Height;
            ExtentsInModel = building.ExtentsInModel;
            YMax = ExtentsInModel.MaxPoint.Y;
            YMin = ExtentsInModel.MinPoint.Y;            
        }

        /// <summary>
        /// Инициализация контура для работы с ним (открытие объекта контура и копирование в модель)
        /// </summary>
        public void InitContour()
        {
            Contour = Building.GetContourInModel();
        }
    }
}
