using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using MicroMvvm;
using PIK_GP_Acad.Elements.Buildings;
using PIK_GP_Acad.Insolation.Models;

namespace PIK_GP_Acad.Insolation.UI
{
    public class InsPointViewModel : ViewModelBase
    {
        private IBuilding ibuild;
        public InsPointViewModel (InsPoint insPoint)
        {
            BuildingTypes = new ObservableCollection<BuildingTypeEnum> { BuildingTypeEnum.Living, BuildingTypeEnum.Social };
            InsPoint = insPoint;
            ibuild = insPoint.Building.Building;

            BuildingType = insPoint.Building.BuildingType;
            Height = insPoint.Height;
            WindowVM = new WindowOptionsViewModel(insPoint.Window.Copy());

            OK = new RelayCommand(OnOkExecute);            
        }

        /// <summary>
        /// Модель
        /// </summary>
        public InsPoint InsPoint { get; set; }

        public RelayCommand OK { get; set; }        

        public ObservableCollection<BuildingTypeEnum> BuildingTypes { get; set; }    
            
        public BuildingTypeEnum BuildingType {
            get { return buildingType; }
            set { buildingType = value; RaisePropertyChanged(); }
        }
        BuildingTypeEnum buildingType;   
        
        public int Height {
            get { return height; }
            set { height = value; RaisePropertyChanged(); }
        }
        int height;

        public WindowOptionsViewModel WindowVM { get { return windowVM; } set { windowVM = value; RaisePropertyChanged(); } }
        WindowOptionsViewModel windowVM;

        private void OnOkExecute ()
        {
            InsPoint.Building.BuildingType = BuildingType;
            InsPoint.Height = Height;
            InsPoint.Window = WindowVM.Window;
        }        
    }
}
