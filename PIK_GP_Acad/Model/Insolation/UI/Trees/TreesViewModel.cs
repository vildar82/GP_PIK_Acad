using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using PIK_GP_Acad.Insolation.Options;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PIK_GP_Acad.Insolation.Trees;
using Catel.MVVM;
using Catel.Fody;

namespace PIK_GP_Acad.Insolation.UI
{
    public class TreesViewModel : ViewModelBase
    {
        static ImageSource imageTree = BitmapFrame.Create(new Uri("pack://application:,,,/PIK_GP_Acad;component/Resources/trees.png"));            

        public TreesViewModel (TreeModel treeModel)
        {
            TreeModel = treeModel;
        }        

        [Model]
        [Expose("VisualOptions")]
        [Expose("Points")]
        public TreeModel TreeModel { get; set; }                

        private Command addPoint;
        public Command AddPoint {
            get {
                return addPoint ?? (addPoint = new Command(() =>
                {
                    TreeModel.NewPoint();
                }, () => true));
            }
        }               
    }    
}
