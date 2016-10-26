using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using PIK_GP_Acad.Insolation.Services;
using PIK_GP_Acad.Insolation.Models;
using MicroMvvm;

namespace PIK_GP_Acad.Insolation.UI
{
    public class InsViewModel : ViewModelBase
    {
        public InsViewModel ()
        {            
            SelectRegion = new RelayCommand(OnSelectRegionExecute);
            Update = new RelayCommand(OnUpdateExecute);
        }
        
        //[Expose("Options")]
        //[Expose("Tree")]
        //[Expose("IsUpdateRequired")]
        //[Expose("UpdateInfo")]
        public InsModel Model { get; set; }
        public RelayCommand SelectRegion { get; private set; }
        public RelayCommand Update { get; private set; }        

        private void OnSelectRegionExecute ()
        {            
            var regionViewModel = new InsRegionViewModel(Model.Options.Region);                        
            if (InsService.ShowDialog(regionViewModel) == true)
            {
                Model.Options.Region = regionViewModel.InsRegion;
                Model.Update();                
            }
            else
            {
                // Отмена изменений ???
            }
        }

        private void OnUpdateExecute ()
        {
            Model?.Update();
        }
    }
}
