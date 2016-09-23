namespace PIK_GP_Acad.Insolation.UI.Trees
{
    using Catel.MVVM;
    using System.Threading.Tasks;
    using Insolation.Trees;
    using System.Collections.Generic;
    using Catel.Fody;

    public class WindowOptionsViewModel : ViewModelBase
    {
        public WindowOptionsViewModel (WindowOptions windowOption) : base()
        {
            WindowOptions = windowOption;
            WindowConstructions = WindowConstruction.WindowConstructions;
            Quarters = new List<double> { 0.07, 0.13, 0.26 };
        }        

        [Model]
        [Expose("Width")]
        [Expose("Quarter")]        
        public WindowOptions WindowOptions { get; set; }

        [ViewModelToModel]
        public WindowConstruction Construction { get; set; }
        public double ShadowAngle { get { return WindowOptions.GetShadowAngle(); } }
        public List<double> Quarters { get; set; } 
        public List<WindowConstruction> WindowConstructions { get; set; }
    }
}
