using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.Insolaion.Constructions
{
    /// <summary>
    ///  Окружающая застройка
    /// </summary>
    public class Surround : Building
    {
        public override Extents3d ExtentsInModel { get; set; }
        public Surround (Polyline pl)
        {
            ExtentsInModel = pl.GeometricExtents;            
        }        
    }
}
