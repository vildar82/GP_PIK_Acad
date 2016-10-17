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
        public TreesViewModel (TreeModel treeModel)
        {
            TreeModel = treeModel;
            AddPoint = new TaskCommand(OnAddPointExecute, OnAddPointCanExecute);
            ShowPoint = new TaskCommand(OnShowPointExecute);
            EditPoint = new TaskCommand(OnEditPointExecute, OnEditPointCanExecute);
            DeletePoint = new TaskCommand(OnDeletePointExecute);            
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
        public TaskCommand EditPoint { get; private set; }
        public TaskCommand DeletePoint { get; private set; }

        private async Task OnAddPointExecute ()
        {
            // Выбор точки на чертеже и задание параметров окна
            SelectPoint selPt = new SelectPoint();
            InsPoint p = selPt.SelectNewPoint(TreeModel.Model);
            // Расчет и добавление точки
            TreeModel.AddPoint(p);
            // Включение зон инсоляции точки
            p.IsVisualIllumsOn = true;
            // Сохранение точки
            p.SaveInsPoint();            
        }        

        private bool OnAddPointCanExecute ()
        {
            bool res;
            if (TreeModel?.Model?.Map?.Buildings.Count > 0)
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

        private async Task OnEditPointExecute ()
        {
            var insPoint = SelectedPoint;
            if (insPoint == null) return;

            var building = insPoint.Building;
            if (building == null) return;

            var oldBuildingType = building.BuildingType;            

            var insPointVM = new InsPointViewModel(insPoint);
            var uiVisualizerService = ServiceLocator.Default.ResolveType<IUIVisualizerService>();
            if (await uiVisualizerService.ShowDialogAsync(insPointVM) == true)
            {
                // Если измениля тип здания - то пересчет всех точек на этом здании
                if (oldBuildingType != building.BuildingType)
                {
                    //// Учет изменения типа здания для всех точек на этом здании                    
                    TreeModel.Model.ChangeBuildingType(building);                    
                }
                else
                {
                    // Обновление только этой точки
                    insPoint.Update();
                }

                // Обновление елочек
                TreeModel.UpdateVisualTree(insPoint);

                // Сохранение точки в словарь
                insPoint.SaveInsPoint();
            }             
        }
        private bool OnEditPointCanExecute ()
        {
            return SelectedPoint?.Building != null;
        }

        private async Task OnDeletePointExecute ()
        {
            SelectedPoint?.Delete();
        }
    }
}
