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
    public class Classificator : IClassificator
    {
        public ObjectId IdEnt { get; set; }
        public ClassType ClassType { get; set; }                
        public Error Error { get; set; }

        public Classificator (ObjectId idEnt, ClassType classType)
        {
            IdEnt = idEnt;
            ClassType = classType;            
        }
    }
}
