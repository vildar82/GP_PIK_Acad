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
using PIK_GP_Acad.Insolation.Models;

namespace PIK_GP_Acad.Insolation.UI.Trees
{
    public class TreesViewModel : ViewModelBase, IInsCalcViewModel
    {
        static ImageSource imageTree = BitmapFrame.Create(new Uri("pack://application:,,,/PIK_GP_Acad;component/Resources/trees.png"));            

        public TreesViewModel ()
        {            
            VisualOptions = InsOptions.DefaultVisualOptions();           
        }        

        [Model]
        public TreeModel TreeModel { get; set; }

        public string Name { get; } = "Trees";

        public List<VisualOption> VisualOptions { get; set; }        

        private Command addPoint;
        public Command AddPoint {
            get {
                return addPoint ?? (addPoint = new Command(() =>
                {
                    // Запрос точки у пользователя.                    
                }, () => true));
            }
        }       

        public void Update (RegionOptions selectedCity)
        {
            throw new NotImplementedException();
        }
    }    
}
