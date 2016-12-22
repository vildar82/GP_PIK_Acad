namespace PIK_GP_Acad.Insolation.UI
{
    public partial class WindowOptionsView
    {
        public WindowOptionsView() 
        {
            InitializeComponent();
        }
        public WindowOptionsView (WindowOptionsViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
