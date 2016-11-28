using System.Windows.Controls;

namespace PIK_GP_Acad.Insolation.UI
{
    public partial class TreeOptionsView
    {
        public TreeOptionsView ()
            : this(null) { }

        public TreeOptionsView (TreeOptionsViewModel vm)            
        {            
            InitializeComponent();
            DataContext = vm;
        }       

        private void Ok_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Button_GiveFeedback(object sender, System.Windows.GiveFeedbackEventArgs e)
        {

        }
    }
}
