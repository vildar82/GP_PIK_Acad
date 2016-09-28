using Catel.MVVM;
using System.Threading.Tasks;
using System.Collections.Generic;
using Catel.Fody;
using PIK_GP_Acad.Insolation.Models;
using System.Collections.ObjectModel;
using Catel.Windows.Interactivity;
using Catel.Data;

namespace PIK_GP_Acad.Insolation.UI
{
    public class WindowOptionsViewModel : ViewModelBase
    {
        public WindowOptionsViewModel (WindowOptions windowOption) : base()
        {
            WindowOptions = windowOption;
            WindowConstructions = WindowConstruction.WindowConstructions;
            Quarters = new ObservableCollection<double> { 0.07, 0.13, 0.26 };            
        }        

        [Model]
        [Expose("Width")]
        [Expose("Quarter")]
        [Expose("Construction")]
        [Expose("ShadowAngle")]
        [Expose("IsCustomAngle")]
        public WindowOptions WindowOptions { get; set; }                
        public ObservableCollection<double> Quarters { get; set; } 
        public ObservableCollection<WindowConstruction> WindowConstructions { get; set; }
    }
}
