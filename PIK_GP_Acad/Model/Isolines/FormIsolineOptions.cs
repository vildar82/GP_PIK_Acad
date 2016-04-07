using System.Windows.Forms;

namespace PIK_GP_Acad.Isolines
{
   public partial class FormIsolineOptions : Form
   {
      public IsolineOptions IsolineOptions { get; set; }

      public FormIsolineOptions(IsolineOptions isolineOptions)
      {
         InitializeComponent();
         IsolineOptions = isolineOptions;
         propertyGrid1.SelectedObject = IsolineOptions;
      }
   }
}