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
            // Выбор точки на чертеже и задание параметров окна
            SelectPoint selPt = new SelectPoint();
            InsPoint p = selPt.SelectNewPoint(TreeModel.Model);
            // Расчет и добавление точки
            TreeModel.AddPoint(p);
            // Включение зон инсоляции точки
            p.IsVisualIllumsOn = true;
            // Сохранение точки
            InsExtDataHelper.Save(p, TreeModel.Model.Doc);
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
    }
}
