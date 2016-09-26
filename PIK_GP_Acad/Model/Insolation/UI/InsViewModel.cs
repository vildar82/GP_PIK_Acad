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
using PIK_GP_Acad.Insolation.Options;
using Catel.Services;
using PIK_GP_Acad.Insolation.Services;

namespace PIK_GP_Acad.Insolation.UI
{
    public class InsViewModel : ViewModelBase
    {        
        public InsViewModel(InsModel insModel) : base()
        {
            InsModel = insModel;            
        }        

        [Model]
        [Expose("Options")]
        [Expose("Tree")]        
        public InsModel InsModel { get; set; }                


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

        Command showRegion;
        public Command ShowRegion {
            get {
                return showRegion ?? (showRegion = new Command(() =>
                {
                    var regionViewModel = new InsRegionViewModel(InsModel.Options.Region);
                    if (InsService.UIVisualizerService.ShowDialog(regionViewModel) == true)
                    {
                        // TODO: Изменение региона
                    }
                }));
            }
        }
    }
}
