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
using Catel.IoC;
using Catel.Services;

namespace PIK_GP_Acad.Insolation.UI
{
    public class TreesViewModel : ViewModelBase
    {
        public TreesViewModel () : this(null)
        { }
        public TreesViewModel (TreeModel treeModel)
        {
            TreeModel = treeModel;
            AddPoint = new TaskCommand(OnAddPointExecute, OnAddPointCanExecute);
            ShowPoint = new TaskCommand(OnShowPointExecute);
        }        

        [Model]        
        [Expose("Points")]
        [Expose("IsVisualIllumsOn")]
        [Expose("IsVisualTreeOn")]        
        public TreeModel TreeModel { get; set; }  

        public InsPoint SelectedPoint { get; set; }
        public string AddpointInfo { get; set; }

        public TaskCommand AddPoint { get; private set; }
        public TaskCommand ShowPoint { get; private set; }

        private async Task OnAddPointExecute ()
        {
            TreeModel.AddPoint();
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

        private async Task OnShowPointExecute ()
        {
            TreeModel.ShowPoint(SelectedPoint);
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
                        MaxContinuosTime =150, TotalTime= 280, Requirement = new InsRequirement () {
                            Type = InsRequirementEnum.C, Color = System.Drawing.Color.Green } },
                    Building = new InsBuilding () { BuildingType = Elements.Buildings.BuildingTypeEnum.Living } }                    
            };                        
        }
    }
}
