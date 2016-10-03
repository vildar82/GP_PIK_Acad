using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Catel.Fody;
using Catel.MVVM;
using Catel.IoC;
using PIK_GP_Acad.Insolation;
using Catel.Services;
using PIK_GP_Acad.Insolation.Services;
using PIK_GP_Acad.Insolation.Models;

namespace PIK_GP_Acad.Insolation.UI
{
    public class InsViewModel : ViewModelBase
    {
        public InsViewModel (InsModel insModel) : base()
        {
            InsModel = insModel;
            SelectRegion = new TaskCommand(OnSelectRegionExecute);
        }

        [Model]
        [Expose("Options")]
        [Expose("Tree")]
        [Expose("IsInsActivated")]
        public InsModel InsModel { get; set; }
        public TaskCommand SelectRegion { get; private set; }

        protected override async Task InitializeAsync ()
        {
            await base.InitializeAsync();

            // TODO: subscribe to events here            
        }

        protected override async Task CloseAsync ()
        {
            // TODO: unsubscribe from events here

            await base.CloseAsync();
        }        

        private async Task OnSelectRegionExecute ()
        {
            var regionViewModel = new InsRegionViewModel(InsModel.Options.Region);
            var uiVisualizerService = ServiceLocator.Default.ResolveType<IUIVisualizerService>();
            if (await uiVisualizerService.ShowDialogAsync(regionViewModel) == true)
            {
                InsModel.Options.Region = regionViewModel.InsRegion;
            }
        }  
    }
}
