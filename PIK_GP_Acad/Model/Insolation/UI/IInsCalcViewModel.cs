using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroMvvm;
using PIK_GP_Acad.Insolation.Options;

namespace PIK_GP_Acad.Insolation.UI
{
    public interface IInsCalcViewModel
    {
        string Name { get; }

        void Update (RegionOptions selectedCity);
    }
}
