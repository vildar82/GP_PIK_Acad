using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Acad.FCS
{
    public interface IClassificator
    {
        ObjectId IdEnt { get;  }
        ClassType ClassType { get; }        
        double Value { get;  }
    }
}
