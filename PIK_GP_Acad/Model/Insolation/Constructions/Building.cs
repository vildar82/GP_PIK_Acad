using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace PIK_GP_Acad.Insolation.Constructions
{
    /// <summary>
    /// Здание - по контуру полилинии
    /// </summary>
    public abstract class Building : IBuilding
    {
        public abstract Extents3d ExtentsInModel { get; set; }

        public abstract Polyline GetCurve ();
    }
}
