using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Errors;
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
        public Entity ContourInModel { get; set; }
        public List<FCProperty> FCProperties { get; set; }
        public int Height { get; set; }  
        public Error Error { get; set; }        
        public ClassType ClassType { get; set; }

        public Building (Entity ent, int height, List<FCProperty> props, ClassType classType)
        {
            IdEnt = ent.Id;
            ExtentsInModel = ent.GeometricExtents;
            ContourInModel = ent;
            ClassType = classType;
            FCProperties = props;
            Floors = props.GetPropertyValue<int>(PropFloors, IdEnt, false);
            Height = height;            
        }       
    }
}
