using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIK_GP_Acad.Insolation.Models;

namespace PIK_GP_Acad.Insolation.Services
{
    public interface ICalcTrees
    {
        List<IIlluminationArea> CalcPoint (InsPoint insPoint, bool withOwnerBuilding = true);
    }
}
