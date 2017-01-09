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
        private static WindowOptions defaultWindow = WindowOptions.Default;
        private IBuilding build;
        public InsPointViewModel (InsPoint insPoint)
        {
            BuildingTypes = new ObservableCollection<BuildingTypeEnum> { BuildingTypeEnum.Living, BuildingTypeEnum.Social };
            InsPoint = insPoint;

            // Точка не на здании, а в свободном месте (типа расчета площадки), окна - нет! 
            if (insPoint.Building == null)
            {
                HasBuilding = false;
                WindowVM = new WindowOptionsViewModel(null);
            }
            else
            {
                HasBuilding = true;
                build = insPoint.Building.Building;
                BuildingType = insPoint.Building.BuildingType;
                if (WindowOptions.Default == insPoint.Window)
                {
                    WindowVM = new WindowOptionsViewModel(defaultWindow);
                }
                else
                {
                    WindowVM = new WindowOptionsViewModel(insPoint.Window);
                }
            }                       
            Height = insPoint.Height;            
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
        
        public double Height {
            get { return height; }
            set { height = value; RaisePropertyChanged(); }
        }
        double height;

        public WindowOptionsViewModel WindowVM { get { return windowVM; } set { windowVM = value; RaisePropertyChanged(); } }
        WindowOptionsViewModel windowVM;

        public bool HasBuilding { get; set; }

        private void OnOkExecute ()
        {
            if (HasBuilding)
            {
                InsPoint.Building.BuildingType = BuildingType;
                InsPoint.Window = WindowVM.Window;
                defaultWindow = WindowVM.Window;
            }            
            InsPoint.Height = Height;            
        }        
    }
}
