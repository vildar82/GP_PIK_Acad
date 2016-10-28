namespace PIK_GP_Acad.Insolation.UI
{
    public partial class InsRegionView
    {
        public InsRegionView () : this(null) { }

        public InsRegionView (InsRegionViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void OkButtonClick (object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
