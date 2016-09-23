using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using MicroMvvm;
using PIK_GP_Acad.Insolation.Options;

namespace PIK_GP_Acad.Insolation.UI
{
    public interface IInsCalcViewModel
    {
        string Name { get; }
        System.Windows.UIElement Header { get; }
        System.Windows.UIElement Content { get; }

        void Update (RegionOptions selectedCity);
    }
}
