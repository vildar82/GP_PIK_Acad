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
        static List<BuildingTypeName> buildingTypes = new List<BuildingTypeName> {
            new BuildingTypeName { Type= BuildingTypeEnum.Living, Name = "Жилое" },
            new BuildingTypeName { Type = BuildingTypeEnum.Social, Name = "Социальное" }
        };        

        public InsPointViewModel (InsPoint insPoint): base()
        {            
            InsPoint = insPoint;            
            SelectedBuildingType = GetBuildingTypeNameByType(InsPoint.Building.BuildingType);            
        }        

        [Model]
        [Expose("Number")]
        [Expose("Building")]
        [Expose("Height")]  
        [Expose("Window")]   
        [Expose("InsValue")]    
        public InsPoint InsPoint { get; set; }
        public List<BuildingTypeName> BuildingTypes { get; set; } = buildingTypes;
        public BuildingTypeName SelectedBuildingType { get; set; }        

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

        private void OnSelectedBuildingTypeChanged()
        {
            InsPoint.Building.BuildingType = SelectedBuildingType.Type;
        }

        private BuildingTypeName GetBuildingTypeNameByType (BuildingTypeEnum buildingType)
        {
            var res = buildingTypes.Find(f => f.Type == buildingType);
            return res;
        }        
    }

    public class BuildingTypeName : ModelBase
    {
        public BuildingTypeEnum Type { get; set; }
        public string Name { get; set; }
    }         
}
