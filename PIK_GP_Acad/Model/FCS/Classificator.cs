using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Errors;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.Elements;

namespace PIK_GP_Acad.FCS
{
    /// <summary>
    /// Классифицированный объект. - Пока только контур или штриховка площадного объекта (участка)
    /// </summary>
    public class Classificator : IClassificator, IArea
    {
        public ObjectId IdEnt { get; set; }
        public ClassType ClassType { get; set; }                
        public Error Error { get; set; }
        public double Area { get; set; }
        public string Layer { get; }

        public Classificator (Entity ent, ClassType classType, double area)
        {
            Layer = ent.Layer;
            IdEnt = ent.Id;
            ClassType = classType;
            Area = area;
        }
    }
}
