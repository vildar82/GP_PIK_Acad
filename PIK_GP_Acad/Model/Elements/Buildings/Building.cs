using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Errors;
using AcadLib.XData;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.FCS;

namespace PIK_GP_Acad.Elements.Buildings
{
    /// <summary>
    /// Здание - по классифицированному контуру полилинии
    /// </summary>
    public class Building : IBuilding, IClassificator
    {
        public const string PropHeight = "Высота";
        public const string PropFloors = "Этажность";

        public ObjectId IdEnt { get; set; }
        public int Floors { get; set; }
        public Extents3d ExtentsInModel { get; set; }        
        public List<FCProperty> FCProperties { get; set; }
        public int Height { get; set; }  
        public Error Error { get; set; }        
        public ClassType ClassType { get; set; }
        public BuildingTypeEnum BuildingType { get; set; }
        public string HouseName { get; set; }

        public string PluginName { get; set; }

        public Building (Entity ent, int height, List<FCProperty> props, ClassType classType)
        {
            IdEnt = ent.Id;
            ExtentsInModel = ent.GeometricExtents;            
            ClassType = classType;
            FCProperties = props;
            Floors = props.GetPropertyValue<int>(PropFloors, IdEnt, false);
            Height = height;            
        }
        public Polyline GetContourInModel()
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

        public DBObject GetDBObject ()
        {
            throw new NotImplementedException();
        }

        public DicED GetExtDic (Document doc)
        {
            throw new NotImplementedException();
        }

        public void SetExtDic (DicED dicEd, Document doc)
        {
            throw new NotImplementedException();
        }

        public List<TypedValue> GetDataValues (Document doc)
        {
            throw new NotImplementedException();
        }

        public void SetDataValues (List<TypedValue> values, Document doc)
        {
            throw new NotImplementedException();
        }
    }
}
