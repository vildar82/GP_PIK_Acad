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
        private IUIVisualizerService uiService;
        public TreesViewModel (TreeModel treeModel, IUIVisualizerService uiService)
        {
            this.uiService = uiService;
            Tree = treeModel;
            AddPoint = new TaskCommand(OnAddPointExecute);
            ShowPoint = new TaskCommand(OnShowPointExecute);
            EditPoint = new TaskCommand<InsPoint>(OnEditPointExecute);
            DeletePoint = new TaskCommand<InsPoint>(OnDeletePointExecute);
            EditTreeOptions = new TaskCommand(OnEditTreeOptionsExecute);
        }

        [Model]        
        [Expose(nameof(TreeModel.Points))]
        [Expose(nameof(TreeModel.IsVisualIllumsOn))]
        [Expose(nameof(TreeModel.IsVisualTreeOn))]        
        public TreeModel Tree { get; set; }  

        public InsPoint SelectedPoint { get; set; }        

        public TaskCommand AddPoint { get; private set; }
        public TaskCommand ShowPoint { get; private set; }
        public TaskCommand<InsPoint> EditPoint { get; private set; }
        public TaskCommand<InsPoint> DeletePoint { get; private set; }
        public TaskCommand EditTreeOptions { get; private set; }

        private async Task OnAddPointExecute ()
        {
            // Выбор точки на чертеже и задание параметров окна
            SelectPoint selPt = new SelectPoint();
            InsPoint p = selPt.SelectNewPoint(Tree.Model);
            // Расчет и добавление точки
            Tree.AddPoint(p);
            // Включение зон инсоляции точки
            p.IsVisualIllumsOn = true;
            // Сохранение точки
            p.SaveInsPoint();            
        }        
        
        private async Task OnShowPointExecute ()
        {
            Tree.ShowPoint(SelectedPoint);
        }

        private async Task OnEditPointExecute (InsPoint insPoint)
        {            
            if (insPoint == null) return;

            var building = insPoint.Building;
            if (building == null) return;

            var oldBuildingType = building.BuildingType;            

            var insPointVM = new InsPointViewModel(insPoint);
            //var uiVisualizerService = ServiceLocator.Default.ResolveType<IUIVisualizerService>();
            if (await uiService.ShowDialogAsync(insPointVM) == true)
            {
                // Если измениля тип здания - то пересчет всех точек на этом здании
                if (oldBuildingType != building.BuildingType)
                {
                    //// Учет изменения типа здания для всех точек на этом здании                    
                    Tree.Model.ChangeBuildingType(building);                    
                }
                else
                {
                    // Обновление только этой точки
                    insPoint.Update();
                }

                // Обновление елочек
                Tree.UpdateVisualTree(insPoint);

                // Сохранение точки в словарь
                insPoint.SaveInsPoint();
            }             
        }

        private async Task OnDeletePointExecute (InsPoint insPoint)
        {
            insPoint.Delete();            
        }

        private async Task OnEditTreeOptionsExecute ()
        {
            var treeOptionsVM = new TreeOptionsViewModel(Tree.TreeOptions);
            //var uiVisualizerService = ServiceLocator.Default.ResolveType<IUIVisualizerService>();
            if (await uiService.ShowDialogAsync(treeOptionsVM) == true)
            {
                // Обновление расчета елочек
                Tree.Update();
            }
        }
    }
}
