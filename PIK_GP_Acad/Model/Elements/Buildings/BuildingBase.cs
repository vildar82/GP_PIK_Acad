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

namespace PIK_GP_Acad.Elements.Buildings
{
    public class BuildingBase : IBuilding
    {
        public BuildingTypeEnum BuildingType { get; set; }
        public Error Error { get; set; }
        public Extents3d ExtentsInModel { get; set; }
        public int Floors { get; set; }
        public int Height { get; set; }
        public string HouseName { get; set; }
        public ObjectId IdEnt { get; set; }
        public string PluginName { get; set; } = "GP";

        public BuildingBase (ObjectId idEnt)
        {
            IdEnt = idEnt;
        }

        public virtual Polyline GetContourInModel()
        {
            throw new NotImplementedException();
        }

        public DBObject GetDBObject()
        {
            return IdEnt.GetObject(OpenMode.ForWrite, false, true);
        }

        public DicED GetExtDic(Document doc)
        {
            var dicBuild = new DicED("Building");
            dicBuild.AddRec("Values", GetDataValues(doc));
            return dicBuild;
        }

        public void SetExtDic(DicED dicEd, Document doc)
        {
            if (dicEd == null) return;
            var dicBuild = dicEd.GetInner("Building");
            SetDataValues(dicBuild.GetRec("Values")?.Values, doc);
        }

        public List<TypedValue> GetDataValues(Document doc)
        {
            return new List<TypedValue> {
                TypedValueExt.GetTvExtData(HouseName)
            };
        }

        public void SetDataValues(List<TypedValue> values, Document doc)
        {
            if (values == null || values.Count != 1)
            {
                // Дефолт
            }
            else
            {
                HouseName = TypedValueExt.GetTvValue<string>(values[0]);
            }
        }
    }
}
