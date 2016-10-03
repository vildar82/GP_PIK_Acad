using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using PIK_GP_Acad.Insolation;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Catel.MVVM;
using Catel.Fody;
using PIK_GP_Acad.Insolation.Models;
using PIK_GP_Acad.Insolation.Services;

namespace PIK_GP_Acad.Insolation.UI
{
    public class TreesViewModel : ViewModelBase
    {
        public TreesViewModel () { }
        public TreesViewModel (TreeModel treeModel)
        {
            TreeModel = treeModel;
            AddPoint = new TaskCommand(OnAddPointExecute, OnAddPointCanExecute);
        }        

        [Model]
        [Expose("VisualOptions")]
        [Expose("Points")]
        [Expose("IsVisualIllumsOn")]
        [Expose("IsVisualTreeOn")]        
        public TreeModel TreeModel { get; set; }  

        public InsPoint SelectedPoint { get; set; }

        public string AddpointInfo { get; set; }

        public TaskCommand AddPoint { get; private set; }

        private async Task OnAddPointExecute ()
        {
            TreeModel.NewPoint();
        }

        private bool OnAddPointCanExecute ()
        {
            bool res;
            if (TreeModel?.InsModel?.Map?.Buildings.Count > 0)
            {
                AddpointInfo = "Выбор расчетной точки на контуре здания";
                res = true;
            }
            else
            {
                res = false;
                AddpointInfo = "В чертеже нет зданий";                
            }            
            return res;
        }
    }    

    public class DesignTreesViewModel : TreesViewModel
    {
        public DesignTreesViewModel()
        {
            TreeModel = new TreeModel();
            TreeModel.Points = new ObservableCollection<InsPoint>() {
                new InsPoint(null) {
                    InsValue = new InsValue () {
                        MaxContinuosTime =120, TotalTime= 240, Requirement = new InsRequirement () {
                            Type = InsRequirementEnum.C, Color = System.Drawing.Color.Green } },
                    Building = new InsBuilding () { BuildingType = Elements.Buildings.BuildingTypeEnum.Living } }                    
            };            
            TreeModel.VisualOptions = Settings.DefaultTreeVisualOptions();            
        }
    }
}
