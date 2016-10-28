namespace PIK_GP_Acad.Insolation.UI
{
    public partial class InsPointView
    {   
        public InsPointView() : this(null)
        {
        }

        public InsPointView (InsPointViewModel vm)            
        {            
            InitializeComponent ();
            DataContext = vm;
        }

        private void OkButtonClick (object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
