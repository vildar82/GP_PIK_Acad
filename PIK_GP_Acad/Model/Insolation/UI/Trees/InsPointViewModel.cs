using System;
using System.Collections.Generic;
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
        public InsPointViewModel (InsPoint insPoint): base()
        {            
            InsPoint = insPoint;                        
        }        

        [Model]
        [Expose("Number")]
        [Expose("Building")]
        [Expose("Height")]  
        [Expose("Window")]   
        [Expose("InsValue")]    
        public InsPoint InsPoint { get; set; }
        public List<BuildingTypeEnum> BuildingTypes { get; set; } =
            new List<BuildingTypeEnum> { BuildingTypeEnum.Living, BuildingTypeEnum.Social };        

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
    }
}
