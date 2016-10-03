using Catel.Windows;

namespace PIK_GP_Acad.Insolation.UI
{
    public partial class InsPointView
    {   
        public InsPointView() : this(null)
        {

        }

        public InsPointView (InsPointViewModel viewModel)
            : base(viewModel)
        {            
            InitializeComponent ();
        }
    }
}
