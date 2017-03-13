using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Errors;
using AcadLib.RTree.SpatialIndex;
using AcadLib.XData;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.FCS;
using AcadLib;
using AcadLib.Extensions;
using PIK_GP_Acad.Elements.Blocks.BlockSection;
using NetLib;
using AcadLib.Geometry;

namespace PIK_GP_Acad.Elements.Buildings
{
    /// <summary>
    /// Здание - по классифицированному контуру полилинии
    /// </summary>
    public class Building : BuildingBase, IBuilding, IClassificator
    {
        private Rectangle r;
        /// <summary>
        /// Параметр в OD полилинии по классификатору
        /// </summary>
        public const string PropHeight = "Высота";
        public const string PropElevation = "Уровень";
        public const string PropFloors = "Этажность";
        public const string PropBuildingType = "Тип здания";
        /// <summary>
        /// Классификатор проектируемых зданий
        /// </summary>
        public const string ProjectedBuildingClassName = "Проектируемое_здание";
        public const string SurroundBuildingClassName = "Окружающие_здания";

        public Building(Entity ent, double height, List<FCProperty> props, ClassType classType) : base(ent)
        {            
            ExtentsInModel = ent.GeometricExtents;
            if (ent is Polyline)
            {
                var pl = ent as Polyline;
                if (pl.Area == 0)
                {
                    AddError("Площадь контура не определена.");   
                }
            }
            ClassType = classType;
            FCProperties = props;
            Floors = props.GetPropertyValue(PropFloors, IdEnt, false,0);
            var buildingTypeShortName = props.GetPropertyValue(PropBuildingType, IdEnt, false, "");
            BuildingType = GetBuildingType(buildingTypeShortName);
            Height = CalcHeight(height);
            Elevation = props.GetPropertyValue<double>(PropElevation, IdEnt, false, 0);
            if (ClassType != null)
            {
                IsProjectedBuilding = ClassType.ClassName.EqualsIgroreCaseAndSpecChars(ProjectedBuildingClassName);                
            }
            IsVisible = ent.IsVisibleLayerOnAndUnfrozen();
        }        

        public List<FCProperty> FCProperties { get; set; }               
        public ClassType ClassType { get; set; }

        public override Rectangle Rectangle {
            get {
                if (r == null)
                {
                    r = GetRectangle();
                }
                return r;
            }
            set { r = value; }
        }        

        public override Polyline GetContourInModel()
        {
            using (var ent = IdEnt.Open(OpenMode.ForRead) as Entity)
            {
                if (ent is Polyline)
                {
                    var plCopy = (Polyline)ent.Clone();
                    if (plCopy.Elevation !=0)
                    {
                        plCopy.Elevation = 0;
                    }                    
                    return plCopy;
                }
                else if (ent is Hatch)
                {
                    // Найти контур штриховки и перевести его в полилинию.
                }
            }
            return null;
        }

        private Rectangle GetRectangle()
        {
            if (!IdEnt.IsValidEx()) return null;
            using (var ent = IdEnt.Open(OpenMode.ForRead) as Entity)
            {
                var ext = ent.GeometricExtents;
                return new Rectangle(ext);
            }
        }

        public static BuildingTypeEnum GetBuildingType(string buildingTypeShortName)
        {
            if (string.IsNullOrEmpty(buildingTypeShortName))
                return BuildingTypeEnum.Living;
            switch (buildingTypeShortName.ToLower())
            {
                case "с":
                case "c":
                    return BuildingTypeEnum.Social;                                        
                default:
                    // Пока есть только: ж, с.
                    return BuildingTypeEnum.Living;
            }
        }

        /// <summary>
        /// Проверка что это имя классификатора = Проектируемое здание.
        /// </summary>
        /// <param name="className">Имя классификатора</param>        
        public static bool IsBuildingClass(string className)
        {
            return className.EqualsIgroreCaseAndSpecChars(ProjectedBuildingClassName) ||
                className.EqualsIgroreCaseAndSpecChars(SurroundBuildingClassName);
        }

        /// <summary>
        /// Определение высоты здания. По параметрам высот 1,тип и тех этажа, или = переданной высоте
        /// </summary>
        /// <param name="height">Высота заданная в параметре Высота в классифицированном объекте</param>        
        private double CalcHeight(double height)
        {
            double resHeight;

            // Определение параметров - для расчета инсоляции     
            HeightFirstFloor = FCProperties.GetPropValue(BlockSectionBase.PropHeightFirstFloor, 3.6);
            HeightTypicalFloors = FCProperties.GetPropValue(BlockSectionBase.PropHeightTypicalFloor, 2.9);
            HeightTechnicalFloor = FCProperties.GetPropValue(BlockSectionBase.PropHeightTechFloor, 1.6);

            if (height != 0)
            {
                resHeight = height;
            }
            else
            {                
                resHeight = BlockSectionBase.CalcHeight(HeightFirstFloor, HeightTypicalFloors, HeightTechnicalFloor, Floors);
            }
            return resHeight;
        }        
    }
}
