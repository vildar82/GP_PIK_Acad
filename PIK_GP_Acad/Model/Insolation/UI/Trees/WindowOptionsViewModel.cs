using System.Threading.Tasks;
using System.Collections.Generic;
using PIK_GP_Acad.Insolation.Models;
using System.Collections.ObjectModel;
using MicroMvvm;

namespace PIK_GP_Acad.Insolation.UI
{
    public class WindowOptionsViewModel : ViewModelBase
    {
        public WindowOptionsViewModel (WindowOptions window) : base()
        {
            if (window != null)
            {
                HasWindow = true;
                Window = window;
                WindowConstructions = new ObservableCollection<WindowConstruction>(WindowConstruction.WindowConstructions);
                Quarters = new ObservableCollection<double> { 0.07, 0.13, 0.26 };
            }
        }        

        /// <summary>
        /// Модель
        /// </summary>
        public WindowOptions Window { get; set; }

        public ObservableCollection<double> Quarters { get; set; }
        public ObservableCollection<WindowConstruction> WindowConstructions { get; set; }
        public bool HasWindow { get; set; }
    }
}
