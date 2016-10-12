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
   public partial class FormHorizontalElevation : Form
   {
      public double StartElevation { get; private set; }
      public double StepElevation { get; private set; }

      public FormHorizontalElevation(double startElev , double stepElev )
      {
         InitializeComponent();

         textBoxStartElevation.Text = startElev.ToString();
         textBoxStepElevation.Text = stepElev.ToString();
      }

      private void buttonStart_Click(object sender, EventArgs e)
      {         
         StartElevation = checkDouble(textBoxStartElevation);
         StepElevation = checkDouble(textBoxStepElevation);
      }

      private double checkDouble(TextBox tb)
      {
         double val;
         if (!double.TryParse(tb.Text, out val))
         {
            errorProvider1.SetError(tb, "Должно быть число");
            this.DialogResult = DialogResult.None;
         }
         return val;
      }

      private void buttonoptions_Click(object sender, EventArgs e)
      {
         HorizontalElevationOptions.Show();
      }
   }
}
