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
    }
}
