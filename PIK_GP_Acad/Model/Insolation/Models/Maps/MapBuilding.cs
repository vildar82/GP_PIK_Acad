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
using MicroMvvm;
using Autodesk.AutoCAD.Geometry;

namespace PIK_GP_Acad.Insolation.Models
{
    public class MapBuilding : ModelBase, IVisualElement, IDisposable, IEquatable<MapBuilding>
    {
        public static int IndexesCounter { get; set; }
        public IBuilding Building { get; set; }        
        public Polyline Contour { get; private set; }  
        public Region Region { get; set; }
        /// <summary>
        /// Расчетная высота здания, с учетом уровня
        /// </summary>
        public double HeightCalc { get; set; }
        public double YMax { get; private set; }
        public double YMin { get; private set; }        
        public Extents3d ExtentsInModel { get { return Building.ExtentsInModel; } }
        public BuildingTypeEnum BuildingType { get; set; }        
        public string BuildinTypeName { get { return AcadLib.WPF.Converters.EnumDescriptionTypeConverter.GetEnumDescription(BuildingType); } }
        /// <summary>
        /// Уникальный номер здания в карте
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// Дом которому принадлежит здание
        /// </summary>
        public House House { get; set; }
        public BuildingVisual Visual { get; set; }        

        public MapBuilding () { }

        public MapBuilding(IBuilding building)
        {
            Building = building;
            BuildingType = Building.BuildingType;
            YMax = ExtentsInModel.MaxPoint.Y;
            YMin = ExtentsInModel.MinPoint.Y;
            HeightCalc = building.Height + building.Elevation;
            Index = ++IndexesCounter;
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
            //var ptText = plContour.Centroid();
            var ptText = GetCenter(plContour);

            // Текст описания здания
            var vopt = new VisualOption(System.Drawing.Color.White, ptText);
            var textInfo = VisualHelper.CreateMText(GetInfo(), vopt, 2, AttachmentPoint.MiddleCenter);
            visuals.Add(textInfo);

            //var textInfo = new MText();
            //textInfo.Location = ptText;
            //textInfo.Attachment = AttachmentPoint.MiddleCenter;
            //textInfo.Height = 2;
            //textInfo.Contents = this.GetInfo();
            //textInfo.Color = color;
            //textInfo.Transparency = transp;
            //visuals.Add(textInfo);

            return visuals;
        }

        private Point3d GetCenter(Polyline plContour)
        {
            return plContour.GeometricExtents.Center();
        }

        public override string ToString()
        {
            return GetInfo();
        }

        /// <summary>
        /// Описание здания
        /// </summary>        
        public string GetInfo()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(BuildinTypeName);
            sb.Append("H=").Append(Building.Height.ToString()).AppendLine("м.");
            if (Building.Elevation != 0)
            {
                sb.Append("Уровень=").Append(Building.Elevation.ToString()).AppendLine("м.");
            }
            string projected = Building.IsProjectedBuilding ? "Проектируемое" : "Окр.застройка";
            sb.AppendLine(projected);
            if (!string.IsNullOrEmpty(Building.FriendlyTypeName))
            {
                sb.AppendLine(Building.FriendlyTypeName);
            }
            if (House != null && House.FrontGroup != null)
            {
                sb.Append(House.FrontGroup.Name).Append(", ").AppendLine(House.Name);
            }
#if DEBUG
            sb.Append("BuildingIndex=").Append(Index.ToString()).AppendLine();            
            sb.Append("HouseIndex=").Append(House?.Index ?? 0);
#endif
            return sb.ToString();
        }

        public void UpdateVisual()
        {
            if (Visual == null)
            {
                Visual = new BuildingVisual(this);                
            }
            Visual?.VisualUpdate();
        }

        public void DisposeVisual()
        {
            Visual?.Dispose();
        }

        /// <summary>
        /// Создание временных домов из временных полилиний
        /// </summary>
        /// <param name="plsSecant">Полилинии</param>
        /// <param name="buildingOwner">Здание образец - для заполнения параметров временных домов</param>
        /// <returns></returns>
        internal static List<MapBuilding> CreateFakeBuildings(List<Polyline> plsSecant, MapBuilding buildingOwner)
        {
            var resFakeBuilds = new List<MapBuilding>();
            foreach (var item in plsSecant)
            {
                var fakeBuild = new FakeBuilding(item, buildingOwner.Building);
                var fakeMapBuild = new MapBuilding(fakeBuild);
                resFakeBuilds.Add(fakeMapBuild);
            }
            return resFakeBuilds;
        }

        public void Dispose()
        {
            Contour?.Dispose();
            Region?.Dispose();
            Visual?.Dispose();        
        }

        public bool Equals(MapBuilding other)
        {   
            return Index == other?.Index;                    
        }
        public override int GetHashCode()
        {
            return Index.GetHashCode();
        }
    }
}
