using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Acad.FCS
{
    public interface IFCRow
    {
        string Name { get;  }
        string Units { get;  }
        double Value { get;  }
        ClassType ClassType { get; }
    }
}
