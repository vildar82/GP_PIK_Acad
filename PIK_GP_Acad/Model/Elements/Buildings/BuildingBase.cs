using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Errors;
using AcadLib.XData;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using AcadLib;
using AcadLib.RTree.SpatialIndex;
using PIK_GP_Acad.Insolation.Models;

namespace PIK_GP_Acad.Elements.Buildings
{
    /// <summary>
    /// Базовый абстрактный класс для всех зданий (реализует общее поведение всех зданий - Блок-секций, Соц, Классификаторов.)
    /// </summary>
    public abstract class BuildingBase : IBuilding
    {
        public BuildingBase()
        {

        }
        public BuildingBase(Entity ent)
        {
            IdEnt = ent.Id;
            Layer = ent.Layer;        
            // Загрузка значений из словаря объекта
            this.LoadDboDict();
        }

        public BuildingTypeEnum BuildingType { get; set; }
        public string BuildingTypeName { get { return AcadLib.WPF.Converters.EnumDescriptionTypeConverter.GetEnumDescription(BuildingType); } }
        public Error Error { get; set; }
        public Extents3d ExtentsInModel { get; set; }
        public int Floors { get; set; }
        public double Height { get; set; }        
        public double Elevation { get; set; }
        public int FrontLevel { get; set; }
        public string HouseName { get; set; }
        public int HouseId { get; set; }
        public ObjectId IdEnt { get; set; }
        public string PluginName { get; set; } = "GP";
        public abstract Rectangle Rectangle { get; set; }
        public bool IsProjectedBuilding { get; set; }
        public double HeightFirstFloor { get; set; }
        public double HeightTypicalFloors { get; set; }
        public double HeightTechnicalFloor { get; set; }
        public string FriendlyTypeName { get; set; }
        public string Layer { get; set; }
        public HouseOptions HouseOptions { get; set; }
        public bool IsVisible { get; set; }

        public abstract Polyline GetContourInModel();        

        public DBObject GetDBObject()
        {
            if (!IdEnt.IsValidEx()) return null;
            return IdEnt.GetObject(OpenMode.ForWrite, false, true);
        }

        public DicED GetExtDic(Document doc)
        {
            var dicBuild = new DicED("Building");
            dicBuild.AddRec("Values", GetDataValues(doc));
            dicBuild.AddInner("HouseOptions", HouseOptions?.GetExtDic(doc));
            return dicBuild;
        }

        public void SetExtDic(DicED dicEd, Document doc)
        {
            if (dicEd == null) return;
            var dicBuild = dicEd.GetInner("Building");
            SetDataValues(dicBuild?.GetRec("Values")?.Values, doc);
            var dicHouseOptions = dicBuild?.GetInner("HouseOptions");
            if (dicHouseOptions != null)
            {
                HouseOptions = new HouseOptions();
                HouseOptions.SetExtDic(dicHouseOptions, doc);
            }
        }

        public List<TypedValue> GetDataValues(Document doc)
        {
            var tvk = new TypedValueExtKit();
            tvk.Add("HouseName", HouseName);
            tvk.Add("HouseId", HouseId);
            tvk.Add("FrontLevel", FrontLevel);
            return tvk.Values;
        }

        public void SetDataValues(List<TypedValue> values, Document doc)
        {
            var dictValues = values?.ToDictionary();
            HouseName = dictValues.GetValue("HouseName", "");
            HouseId = dictValues.GetValue("HouseId", 0);
            FrontLevel = dictValues.GetValue("FrontLevel", 0);
        }

        public void AddError(string msg)
        {
            if (Error == null)
            {
                Error = new Error($"Ошибка в объекте здания '{GetInfo()}', слой '{Layer}': ", IdEnt, System.Drawing.SystemIcons.Error);
                Inspector.AddError(Error);
            }
            Error.AdditionToMessage(msg);
        }

        private string GetInfo()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(BuildingTypeName);
            if (Height != 0)
                sb.Append("H=").AppendLine(Height.ToString());
            if (Elevation != 0)            
                sb.Append("Уровень=").AppendLine(Elevation.ToString());
            sb.AppendLine(IsProjectedBuilding ? "Проектируемое" : "Окр.застройка");
            if (!string.IsNullOrEmpty(FriendlyTypeName))            
                sb.AppendLine(FriendlyTypeName);            
            return sb.ToString();
        }

        public double GetLevelHeight(int level)
        {
            if (level > 1)
            {
                if (level > Floors)
                    level = Floors;
                return HeightFirstFloor + ((level - 2) * HeightTypicalFloors);
            }
            return 0;
        }
    }
}
