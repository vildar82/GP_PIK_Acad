using System.Threading.Tasks;
using System.Collections.Generic;
using PIK_GP_Acad.Insolation.Models;
using System.Collections.ObjectModel;
using MicroMvvm;
using System;

namespace PIK_GP_Acad.Insolation.UI
{
    public class WindowOptionsViewModel : ViewModelBase
    {   
        public WindowOptionsViewModel (WindowOptions window)
        {
            if (window != null)
            {                
                HasWindow = true;
                Window = window.Copy();
                WindowConstructions = new ObservableCollection<WindowConstruction>(WindowConstruction.WindowConstructions);
                Quarters = new ObservableCollection<double>(WindowOptions.Quarters);// { 0.07, 0.13, 0.26 };
            }
            Reset = new RelayCommand(OnResetExecute, CanResetExecute);
        }        

        /// <summary>
        /// Модель
        /// </summary>
        public WindowOptions Window { get { return window; } set { window = value; RaisePropertyChanged(); } }
        WindowOptions window;

        public RelayCommand Reset { get; set; }

        public ObservableCollection<double> Quarters { get; set; }
        public ObservableCollection<WindowConstruction> WindowConstructions { get; set; }
        public bool HasWindow { get { return hasWindow; } set { hasWindow = value; RaisePropertyChanged(); } }
        bool hasWindow;

        private bool CanResetExecute()
        {
            var res = !Window?.Equals(WindowOptions.Default) ?? false;
            return res;
        }
        private void OnResetExecute()
        {
            Window = WindowOptions.Default.Copy();
        }        
    }
}
