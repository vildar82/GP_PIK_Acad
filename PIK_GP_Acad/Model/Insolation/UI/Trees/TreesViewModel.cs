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

namespace PIK_GP_Acad.Insolation.UI
{
    public class TreesViewModel : ViewModelBase
    {
        static ImageSource imageTree = BitmapFrame.Create(new Uri("pack://application:,,,/PIK_GP_Acad;component/Resources/trees.png"));

        public TreesViewModel () { }
        public TreesViewModel (TreeModel treeModel)
        {
            TreeModel = treeModel;
            AddPoint = new TaskCommand(OnAddPointExecute, OnAddPointCanExecute);
        }        

        [Model]
        [Expose("VisualOptions")]
        [Expose("Points")]
        public TreeModel TreeModel { get; set; }  

        public string AddpointInfo { get; set; }

        public TaskCommand AddPoint { get; private set; }            
                   
        private async Task OnAddPointExecute()
        {
            TreeModel.NewPoint();
        }

        private bool OnAddPointCanExecute ()
        {
            bool res;
            if (TreeModel.InsModel.Map.Buildings.Count > 0)
            {
                AddpointInfo = "Выбор расчетной точки на здании";
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
}
