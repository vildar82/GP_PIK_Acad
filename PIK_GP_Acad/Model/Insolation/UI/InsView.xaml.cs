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
using System.Windows.Shapes;

namespace PIK_GP_Acad.Insolation.UI
{
    /// <summary>
    /// Логика взаимодействия для InsServiceView.xaml
    /// </summary>
    public partial class InsView
    {
        public InsView (): this(null)
        {

        }

        public InsView (InsViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();            
        }        
    }
}
