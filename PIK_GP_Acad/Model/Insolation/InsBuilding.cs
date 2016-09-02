using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.Elements.Buildings;

namespace PIK_GP_Acad.Insolation
{
    public class InsBuilding
    {
        public IBuilding Building { get; private  set; }
        public Polyline Contour { get; private set; }
        public int Height { get; private set; }
        public double YMax { get; private set; }
        public double YMin { get; private set; }
        public Extents3d ExtentsInModel { get; private set; }

        public InsBuilding(IBuilding building)
        {
            Building = building;
            Contour = building.GetContourInModel();
            Height = building.Height;
            ExtentsInModel = building.ExtentsInModel;
            YMax = ExtentsInModel.MaxPoint.Y;
            YMin = ExtentsInModel.MinPoint.Y;
        }
    }
}
