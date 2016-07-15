using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Acad.Elements;

namespace PIK_GP_Acad.FCS
{
    public interface IClassificator : IElement
    {
        ObjectId IdEnt { get;  }
        ClassType ClassType { get; }                
    }
}
