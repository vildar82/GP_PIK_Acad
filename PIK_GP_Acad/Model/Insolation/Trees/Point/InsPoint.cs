using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;
using Catel.Data;

namespace PIK_GP_Acad.Insolation.Trees
{
    /// <summary>
    /// Расчетная точка
    /// </summary>
    public class InsPoint : ModelBase, IInsPoint
    {
        /// <summary>
        /// Номер точки
        /// </summary>
        public int Number { get; set; }
        public Point3d Point { get; set;}
        public InsBuilding Building { get; set; }        

        public InsPoint()
        {

        }
    }
}
