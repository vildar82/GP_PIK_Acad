using PIK_GP_Acad.Insolation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PIK_GP_Acad.Insolation.UI
{
    /// <summary>
    /// Логика взаимодействия для FrontView.xaml
    /// </summary>
    public partial class FrontView : UserControl
    {
        public FrontView ()
        {
            InitializeComponent();
        }

        private void btnDots_Click(object sender, RoutedEventArgs e)
        {
            cmDots.DataContext = DataContext;
            cmDots.Visibility = Visibility.Visible;
            cmDots.IsOpen = true;
        }
        
        private void rowDoubleClick(object sender, MouseButtonEventArgs e)
        {            
            var row = sender as Control;
            if (row != null)
            {
                var house = row.DataContext as House;
                if (house != null)
                {
                    house.Show();
                }
            }            
        }
    }
}
