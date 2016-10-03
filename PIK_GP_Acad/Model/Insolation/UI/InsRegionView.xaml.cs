using Catel.Windows;

namespace PIK_GP_Acad.Insolation.UI
{
    public partial class InsRegionView
    {
        public InsRegionView () : this(null) { }

        public InsRegionView (InsRegionViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
        }
    }
}
