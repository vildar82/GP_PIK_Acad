using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using PIK_GP_Acad.Insolation.Options;
using MicroMvvm;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PIK_GP_Acad.Insolation.Trees;

namespace PIK_GP_Acad.Insolation.UI.Trees
{
    public class TreesViewModel : ObservableObject, IInsCalcViewModel
    {
        static ImageSource image = BitmapFrame.Create(new Uri("pack://application:,,,/PIK_GP_Acad;component/Resources/trees.png"));            

        public TreesViewModel ()
        {            
            VisualOptions = InsOptions.DefaultVisualOptions();
            Points.CollectionChanged += Points_CollectionChanged;
        }        

        public string Name { get; } = "Trees";

        public List<VisualOption> VisualOptions { get; set; }

        public ObservableCollection<InsPointViewModel> Points { get; set; } = new ObservableCollection<InsPointViewModel>();

        public ICommand AddPoint { get { return new RelayCommand(()=>
        {
            // Запрос точки у пользователя.
            var service = InsServicePallete.CurrentInsServiceViewModel.InsService;
            IInsPoint insPoint = service.Trees.AddPoint();
            if (insPoint != null)
            {
                InsPointViewModel p = new InsPointViewModel(insPoint);
                Points.Add(p);
            }
        }, () => true); } }

        public System.Windows.UIElement Header { get { return new Image { Source = image }; } }

        public System.Windows.UIElement Content { get { return new TreesView(this); } }

        public void Update (RegionOptions selectedCity)
        {
            throw new NotImplementedException();
        }

        private void Points_CollectionChanged (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                for (int i = 0; i < Points.Count; i++)
                {
                    var p = Points[i];
                    if (p.Number != i)
                        p.Number = i;
                }
            }
        }
    }    
}
