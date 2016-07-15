using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.FCS;

namespace PIK_GP_Acad.Elements.Buildings
{
    /// <summary>
    /// Здание - по контуру полилинии
    /// </summary>
    public abstract class Building : IBuilding
    {
        public const string PropHeight = "Высота";
        public const string PropFloors = "Этажность";

        protected ObjectId IdEnt { get; set; }
        public int Floors { get; set; }
        public Extents3d ExtentsInModel { get; set; }
        public Polyline ContourInModel { get; set; }
        public List<FCProperty> FCProperties { get; set; }
        public int Height { get; set; }       

        public Building (Polyline pl, int height, List<FCProperty> props)
        {
            IdEnt = pl.Id;
            ExtentsInModel = pl.GeometricExtents;
            ContourInModel = pl;
            FCProperties = props;
            Floors = FCService.GetPropertyValue<int>(PropFloors, props, IdEnt, false);
            Height = height;            
        }        
    }
}
