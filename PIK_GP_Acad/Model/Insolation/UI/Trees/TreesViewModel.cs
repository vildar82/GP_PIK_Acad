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
using PIK_GP_Acad.Insolation.Models;
using PIK_GP_Acad.Insolation.Services;
using MicroMvvm;

namespace PIK_GP_Acad.Insolation.UI
{
    public class TreesViewModel : ViewModelBase
    {        
        public TreesViewModel (TreeModel treeModel)
        {            
            Tree = treeModel;
            AddPoint = new RelayCommand(OnAddPointExecute);
            ShowPoint = new RelayCommand(OnShowPointExecute);
            EditPoint = new RelayCommand<InsPoint>(OnEditPointExecute);
            DeletePoint = new RelayCommand<InsPoint>(OnDeletePointExecute);
            EditTreeOptions = new RelayCommand(OnEditTreeOptionsExecute);
        }

        //[Model]        
        //[Expose(nameof(TreeModel.Points))]
        //[Expose(nameof(TreeModel.IsVisualIllumsOn))]
        //[Expose(nameof(TreeModel.IsVisualTreeOn))]        
        public TreeModel Tree { get; set; }  

        public InsPoint SelectedPoint { get; set; }        

        public RelayCommand AddPoint { get; private set; }
        public RelayCommand ShowPoint { get; private set; }
        public RelayCommand<InsPoint> EditPoint { get; private set; }
        public RelayCommand<InsPoint> DeletePoint { get; private set; }
        public RelayCommand EditTreeOptions { get; private set; }

        private void OnAddPointExecute ()
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
        
        private void OnShowPointExecute ()
        {
            Tree.ShowPoint(SelectedPoint);
        }

        private void OnEditPointExecute (InsPoint insPoint)
        {            
            if (insPoint == null) return;

            var building = insPoint.Building;
            if (building == null) return;

            var oldBuildingType = building.BuildingType;            

            var insPointVM = new InsPointViewModel(insPoint);
            //var uiVisualizerService = ServiceLocator.Default.ResolveType<IUIVisualizerService>();
            if (InsService.ShowDialog(insPointVM)== true)
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

        private void OnDeletePointExecute (InsPoint insPoint)
        {
            insPoint.Delete();            
        }

        private void OnEditTreeOptionsExecute ()
        {            
            var treeOptionsVM = new TreeOptionsViewModel(Tree.TreeOptions);            
            if (InsService.ShowDialog(treeOptionsVM) == true)
            {
                // Обновление расчета елочек
                Tree.Update();
            }
        }
    }
}
