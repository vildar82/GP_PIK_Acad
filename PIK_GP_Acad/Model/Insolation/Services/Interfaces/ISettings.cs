using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIK_GP_Acad.Insolation.Options;

namespace PIK_GP_Acad.Insolation.Services
{
    public interface ISettings
    {
        ObservableCollection<InsRegion> Regions { get; set; }
        ObservableCollection<TreeVisualOption> TreeVisualOptions { get; set; }
        void Load ();
        void Save ();
    }
}
