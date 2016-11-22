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
        public const string PropElevation = "Относительный уровень";
        public const string PropFloors = "Этажность";       

        public Building(Entity ent, double height, double elevation, List<FCProperty> props, ClassType classType) : base(ent.Id)
        {
            ExtentsInModel = ent.GeometricExtents;
            ClassType = classType;
            FCProperties = props;
            Floors = props.GetPropertyValue<int>(PropFloors, IdEnt, false);
            Height = height;
            Elevation = elevation;
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
    }
}
