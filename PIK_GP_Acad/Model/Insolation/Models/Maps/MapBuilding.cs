﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.Elements.Blocks.BlockSection;
using PIK_GP_Acad.Elements.Buildings;
using PIK_GP_Acad.Insolation.Services;

namespace PIK_GP_Acad.Insolation.Models
{
    public class MapBuilding : ModelBase
    {        
        public IBuilding Building { get; set; }        
        public Polyline Contour { get; private set; }
        public int Height { get; private set; }
        public double YMax { get; private set; }
        public double YMin { get; private set; }        
        public Extents3d ExtentsInModel { get { return Building.ExtentsInModel; } }
        public BuildingTypeEnum BuildingType { get; set; }        
        public string BuildinTypeName { get { return AcadLib.WPF.Converters.EnumDescriptionTypeConverter.GetEnumDescription(BuildingType); } }
        public bool IsSurroundBuilding { get; set; }
        public bool IsProjectBuilding { get; set; }        

        public MapBuilding () { }

        public MapBuilding(IBuilding building)
        {
            Building = building;            
            Height = building.Height;            
            YMax = ExtentsInModel.MaxPoint.Y;
            YMin = ExtentsInModel.MinPoint.Y;                  
            if (building is Building)
            {
                IsSurroundBuilding = true;
            }
            else
            {
                IsProjectBuilding = true;
            }
        }

        /// <summary>
        /// Инициализация контура для работы с ним (открытие объекта контура и копирование в модель)
        /// </summary>
        public void InitContour()
        {
            if (Contour != null && !Contour.IsDisposed)
                Contour.Dispose();
            Contour = Building.GetContourInModel();
        }
    }
}
