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
using AcadLib;
using AcadLib.Statistic;

namespace PIK_GP_Acad.Insolation.UI
{
    public class TreesViewModel : ViewModelBase
    {        
        public TreesViewModel (TreeModel treeModel)
        {            
            Tree = treeModel;
            AddPoint = new RelayCommand(OnAddPointExecute);
            ShowPoint = new RelayCommand<InsPoint>(OnShowPointExecute);
            EditPoint = new RelayCommand<InsPoint>(OnEditPointExecute);
            DeletePoint = new RelayCommand<InsPoint>(OnDeletePointExecute);
            ReportPoint = new RelayCommand<InsPoint>(OnReportPointExecute);
            ReportAllPoints = new RelayCommand(OnReportAllPointsExecute, CanReportAllPointsExecute);
            EditTreeOptions = new RelayCommand(OnEditTreeOptionsExecute);
            DrawVisuals = new RelayCommand(OnDrawVisualsExecute);
        }        

        /// <summary>
        /// Модель
        /// </summary>                       
        public TreeModel Tree { get; set; }          

        public RelayCommand AddPoint { get; private set; }
        public RelayCommand<InsPoint> ShowPoint { get; private set; }
        public RelayCommand<InsPoint> EditPoint { get; private set; }
        public RelayCommand<InsPoint> DeletePoint { get; private set; }
        public RelayCommand<InsPoint> ReportPoint { get; private set; }
        public RelayCommand ReportAllPoints { get; private set; }
        public RelayCommand EditTreeOptions { get; private set; }
        public RelayCommand DrawVisuals { get; private set; }        

        private void OnAddPointExecute ()
        {
            // Выбор точки на чертеже и задание параметров окна
            var selPt = new SelectPoint();
            InsPoint p = selPt.SelectNewPoint(Tree.Model);
            if (p != null)
            {                
                // Расчет и добавление точки
                Tree.AddPoint(p);
                // Включение зон инсоляции точки
                p.IsVisualIllumsOn = true;
                // Сохранение точки
                p.SaveInsPoint();
                // Запись статистики
                PluginStatisticsHelper.AddStatistic();
            }
        }        
        
        private void OnShowPointExecute (InsPoint insPt)
        {
            if ((short)Application.GetSystemVariable("TILEMODE") == 0) return;
            Tree.ShowPoint(insPt);
        }

        private void OnEditPointExecute (InsPoint insPoint)
        {
            var building = insPoint?.Building;
            //if (building == null) return;

            var oldBuildingType = building?.BuildingType ?? Elements.Buildings.BuildingTypeEnum.Living;            

            var insPointVM = new InsPointViewModel(insPoint);
            //var uiVisualizerService = ServiceLocator.Default.ResolveType<IUIVisualizerService>();
            if (InsService.ShowDialog(insPointVM) != true) return;
            // Если измениля тип здания - то пересчет всех точек на этом здании
            if (building != null && oldBuildingType != building.BuildingType)
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
                Tree.UpdateVisual();
            }            
        }

        private void OnReportPointExecute(InsPoint insPt)
        {
            // отчет по точке
        }

        /// <summary>
        /// Рисование визуализации в чертеже
        /// </summary>
        private void OnDrawVisualsExecute ()
        {
            Tree.DrawVisuals();
        }

        private bool CanReportAllPointsExecute()
        {
            return Tree.Points.Any();            
        }
        private void OnReportAllPointsExecute()
        {
            Tree.ReportsAllPoint();
        }
    }
}
