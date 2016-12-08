using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.Elements.Blocks.BlockSection;
using PIK_GP_Acad.Elements.Buildings;
using PIK_GP_Acad.Insolation.Services;
using AcadLib;
using AcadLib.Geometry;
using Autodesk.AutoCAD.Colors;

namespace PIK_GP_Acad.Insolation.Models
{
    public class MapBuilding : ModelBase, IVisualElement
    {        
        public IBuilding Building { get; set; }        
        public Polyline Contour { get; private set; }        
        /// <summary>
        /// Расчетная высота здания, с учетом уровня
        /// </summary>
        public double HeightCalc { get; set; }
        public double YMax { get; private set; }
        public double YMin { get; private set; }        
        public Extents3d ExtentsInModel { get { return Building.ExtentsInModel; } }
        public BuildingTypeEnum BuildingType { get; set; }        
        public string BuildinTypeName { get { return AcadLib.WPF.Converters.EnumDescriptionTypeConverter.GetEnumDescription(BuildingType); } }        

        public MapBuilding () { }

        public MapBuilding(IBuilding building)
        {
            Building = building;
            BuildingType = Building.BuildingType;
            YMax = ExtentsInModel.MaxPoint.Y;
            YMin = ExtentsInModel.MinPoint.Y;
            HeightCalc = building.Height + building.Elevation;                            
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

        /// <summary>
        /// Элементы визуализации здания
        /// </summary>        
        public List<Entity> CreateVisual()
        {
            var visuals = new List<Entity>();

            var color = Color.FromColor(System.Drawing.Color.Violet);
            var transp = new Transparency(100);
            // Контур здания
            Polyline plContour;
            if (Contour == null || Contour.IsDisposed)
            {
                plContour = Building.GetContourInModel();
            }
            else
            {
                plContour = (Polyline)Contour.Clone();
            }
            plContour.ConstantWidth = 0.2;
            plContour.Color = color;
            //plContour.Transparency = transp;
            visuals.Add(plContour);

            // точка вставки текста
            var ptText = plContour.Centroid();

            // Текст описания здания
            var textInfo = new MText();
            textInfo.Location = ptText;
            textInfo.Attachment = AttachmentPoint.MiddleCenter;
            textInfo.Height = 2;
            textInfo.Contents = this.ToString();
            textInfo.Color = color;
            //textInfo.Transparency = transp;
            visuals.Add(textInfo);

            return visuals;
        }

        
        public override string ToString()
        {
            return GetInfo();
        }

        /// <summary>
        /// Описание здания
        /// </summary>        
        private string GetInfo()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(BuildinTypeName);
            sb.Append("H=").AppendLine(Building.Height.ToString());
            if (Building.Elevation != 0)
            {
                sb.Append("Уровень=").AppendLine(Building.Elevation.ToString());
            }
            string projected = Building.IsProjectedBuilding ? "Проектируемое" : "Окр.застройка";
            sb.AppendLine(projected);
            if (!string.IsNullOrEmpty(Building.FriendlyTypeName))
            {
                sb.Append(Building.FriendlyTypeName);
            }

            return sb.ToString();
        }
    }
}
