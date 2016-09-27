using Catel.Windows;

namespace PIK_GP_Acad.Insolation.UI
{
    public partial class InsPointView
    {        
        public InsPointView (InsPointViewModel viewModel)
            : base(viewModel, DataWindowMode.OkCancel, null, DataWindowDefaultButton.OK, true, InfoBarMessageControlGenerationMode.Inline)
        {            
            InitializeComponent ();
        }
    }
}
