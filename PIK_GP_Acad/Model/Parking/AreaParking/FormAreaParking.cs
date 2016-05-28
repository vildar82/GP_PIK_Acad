using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PIK_GP_Acad.Parking
{
    public partial class FormAreaParking : Form
    {
        AreaParking data;        

        public FormAreaParking(AreaParking data)
        {
            InitializeComponent();
            this.data = data;           
                        
            textBoxArea.DataBindings.Add("Text", this.data, nameof(data.Area));

            var binding = textBoxPlaces.DataBindings.Add("Text", this.data, nameof(data.Places));
            binding.DataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged;

            binding = textBoxFloors.DataBindings.Add("Text", this.data, nameof(data.Floors));
            binding.DataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged;
        }

        private void textBoxFloors_TextChanged(object sender, EventArgs e)
        {
            int floors;
            if (int.TryParse(textBoxFloors.Text, out floors))
            {
                data.Floors = floors;
                data.Calc();                
            }
        }

        private void buttonInsertText_Click(object sender, EventArgs e)
        {            
            using (data.Service.Ed.StartUserInteraction(this.Handle))
            {
                data.InsertText();
            }
        }
    }
}
