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
    ///  Окружающая застройка
    /// </summary>
    public class Surround : Building
    {
        private ObjectId idPl;
        public override Extents3d ExtentsInModel { get; set; }
        public Surround (Polyline pl)
        {
            idPl = pl.Id;
            ExtentsInModel = pl.GeometricExtents;            
        }

        public override Polyline GetCurve ()
        {
            var pl = idPl.GetObject(OpenMode.ForRead, false, true) as Polyline;
            return pl;
        }
    }
}
