using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PIK_GP_Acad.KP.Parking.Area
{
    public partial class FormAreaParking : Form
    {
        AreaParking data;        

        public FormAreaParking(AreaParking data)
        {
            InitializeComponent();
            fillComboBoxType();
            this.data = data;           
                        
            textBoxArea.DataBindings.Add("Text", this.data, nameof(data.Area));

            BindingSource bs = new BindingSource();

            var binding = textBoxPlaces.DataBindings.Add("Text", this.data, nameof(data.Places));
            binding.DataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged;            

            binding = textBoxFloors.DataBindings.Add("Text", this.data, nameof(data.Floors));
            binding.DataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged;

            comboBoxType.SelectedIndex = 0;
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

        private void comboBoxType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var parkingType = comboBoxType.SelectedItem as ParkingType;
            if (parkingType != null)
            {
                data.PlaceArea = parkingType.PlaceArea;
                textBoxPlaceArea.Text = parkingType.PlaceArea.ToString();
                if (!parkingType.IsBuilding)
                {
                    data.Floors = 1;
                }
            }
            
            data.Calc();
            textBoxPlaces.Text = data.Places.ToString();
        }        

        private void fillComboBoxType()
        {
            comboBoxType.Items.Add(ParkingType.ParkingBuilding);
            comboBoxType.Items.Add(ParkingType.ParkingBuildingUnderground);
            comboBoxType.Items.Add(ParkingType.ParkingArea);            
        }
    }
}
