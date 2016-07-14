using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.FCS
{
    public class Classificator : IClassificator
    {
        public ObjectId IdEnt { get; set; }
        public ClassType ClassType { get; set; }        
        public double Value { get; set; }               

        public Classificator (ObjectId idEnt, ClassType classType, double value)
        {
            IdEnt = idEnt;
            ClassType = classType;
            Value = Math.Round(value, 2);
        }
    }
}
