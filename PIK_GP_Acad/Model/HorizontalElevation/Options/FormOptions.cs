using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PIK_GP_Acad.HorizontalElevation
{
   public partial class FormOptions : Form
   {
      public HorizontalElevationOptions Options { get; set; }

      public FormOptions(HorizontalElevationOptions options)
      {
         InitializeComponent();

         Options = options;
         propertyGrid1.SelectedObject = options;
      }

      private void buttonOk_Click(object sender, EventArgs e)
      {

      }
   }
}
