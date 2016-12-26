using PIK_GP_Acad.Elements.Buildings;
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

namespace PIK_GP_Acad.Insolation.Models
{
    public class FakeBuilding : BuildingBase
    {
        private Polyline fakePl;
        Rectangle r;
        public FakeBuilding(Polyline fakePl, IBuilding example)
        {
            this.fakePl = fakePl;
            ExtentsInModel = fakePl.GeometricExtents;
            r = new Rectangle(ExtentsInModel);
            BuildingType = example.BuildingType;
            Elevation = example.Elevation;
            Height = example.Height;
            IsProjectedBuilding = example.IsProjectedBuilding;              
        }

        public override Rectangle Rectangle {
            get { return r; }
            set { r = value; }
        }

        public override Polyline GetContourInModel()
        {
            return (Polyline)fakePl.Clone();
        }       
    }
}
