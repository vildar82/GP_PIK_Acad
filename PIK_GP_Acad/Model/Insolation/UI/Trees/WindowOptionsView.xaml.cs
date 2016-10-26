namespace PIK_GP_Acad.Insolation.UI
{
    public partial class WindowOptionsView
    {
        public WindowOptionsView() : this(null)
        {

        }
        public WindowOptionsView (WindowOptionsViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
