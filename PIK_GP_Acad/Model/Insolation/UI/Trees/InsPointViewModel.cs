using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Catel.Data;
using Catel.Fody;
using Catel.MVVM;
using PIK_GP_Acad.Elements.Buildings;
using PIK_GP_Acad.Insolation.Models;

namespace PIK_GP_Acad.Insolation.UI
{
    public class InsPointViewModel : ViewModelBase
    {
        private IBuilding ibuild;
        public InsPointViewModel (InsPoint insPoint): base()
        {
            BuildingTypes = new ObservableCollection<BuildingTypeEnum> { BuildingTypeEnum.Living, BuildingTypeEnum.Social };
            InsPoint = insPoint;
            ibuild = insPoint.Building.Building;                                   
        }        

        [Model]
        [Expose("Number")]        
        [Expose("Height")]  
        [Expose("Window")]   
        [Expose("InsValue")]
        [Expose("Building")]
        public InsPoint InsPoint { get; set; }

        public ObservableCollection<BuildingTypeEnum> BuildingTypes { get; set; }        

        protected override async Task InitializeAsync ()
        {
            await base.InitializeAsync();

            // TODO: subscribe to events here                  
        }        

        protected override async Task CloseAsync ()
        {            
            // TODO: unsubscribe from events here            

            await base.CloseAsync();
            if (InsPoint.Building.Building == null)
            {
                InsPoint.Building.Building = ibuild;
            }
        }        
    }
}
