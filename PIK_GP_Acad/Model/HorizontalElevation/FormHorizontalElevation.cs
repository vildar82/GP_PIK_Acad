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
        public Color Color { get; set; }


        public FormHorizontalElevation(double startElev, double stepElev, Color color)
        {
            InitializeComponent();

            Color = color;
            textBoxStartElevation.Text = startElev.ToString();
            textBoxStepElevation.Text = stepElev.ToString();
            bColor.BackColor = Color;
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
                DialogResult = DialogResult.None;
            }
            return val;
        }

        private void buttonoptions_Click(object sender, EventArgs e)
        {
            HorizontalElevationOptions.Show();
        }

        private void bColor_Click(object sender, EventArgs e)
        {
            var color = HorizontalElevationService.SelectColor(Color);
            if (color != null)
            {
                Color = color;
                bColor.BackColor = color;
            }
        }
    }
}
