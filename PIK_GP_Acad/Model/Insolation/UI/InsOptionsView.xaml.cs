namespace PIK_GP_Acad.Insolation.UI
{
    public partial class InsOptionsView
    {
        public InsOptionsView () : this(null) { }

        public InsOptionsView (InsOptionsViewModel vm)
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
