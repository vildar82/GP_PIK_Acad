namespace PIK_GP_Acad.KP.Parking.Area
{
    partial class FormAreaParking
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.labelFloors = new System.Windows.Forms.Label();
            this.textBoxFloors = new System.Windows.Forms.TextBox();
            this.buttonInsertText = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxArea = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxPlaces = new System.Windows.Forms.TextBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.comboBoxType = new System.Windows.Forms.ComboBox();
            this.textBoxPlaceArea = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelFloors
            // 
            this.labelFloors.AutoSize = true;
            this.labelFloors.Location = new System.Drawing.Point(20, 15);
            this.labelFloors.Name = "labelFloors";
            this.labelFloors.Size = new System.Drawing.Size(62, 13);
            this.labelFloors.TabIndex = 0;
            this.labelFloors.Text = "Этажность";
            // 
            // textBoxFloors
            // 
            this.textBoxFloors.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxFloors.Location = new System.Drawing.Point(101, 12);
            this.textBoxFloors.Name = "textBoxFloors";
            this.textBoxFloors.Size = new System.Drawing.Size(131, 20);
            this.textBoxFloors.TabIndex = 2;
            this.textBoxFloors.TextChanged += new System.EventHandler(this.textBoxFloors_TextChanged);
            // 
            // buttonInsertText
            // 
            this.buttonInsertText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonInsertText.Location = new System.Drawing.Point(234, 148);
            this.buttonInsertText.Name = "buttonInsertText";
            this.buttonInsertText.Size = new System.Drawing.Size(75, 23);
            this.buttonInsertText.TabIndex = 3;
            this.buttonInsertText.Text = "Текст";
            this.toolTip1.SetToolTip(this.buttonInsertText, "Вставка текста машиномест");
            this.buttonInsertText.UseVisualStyleBackColor = true;
            this.buttonInsertText.Click += new System.EventHandler(this.buttonInsertText_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 53);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Площадь, м2";
            // 
            // textBoxArea
            // 
            this.textBoxArea.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxArea.Location = new System.Drawing.Point(101, 50);
            this.textBoxArea.Name = "textBoxArea";
            this.textBoxArea.ReadOnly = true;
            this.textBoxArea.Size = new System.Drawing.Size(131, 20);
            this.textBoxArea.TabIndex = 2;
            this.toolTip1.SetToolTip(this.textBoxArea, "Площадб полилинии");
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 119);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Машиномест";
            // 
            // textBoxPlaces
            // 
            this.textBoxPlaces.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPlaces.Location = new System.Drawing.Point(101, 116);
            this.textBoxPlaces.Name = "textBoxPlaces";
            this.textBoxPlaces.ReadOnly = true;
            this.textBoxPlaces.Size = new System.Drawing.Size(131, 20);
            this.textBoxPlaces.TabIndex = 2;
            this.toolTip1.SetToolTip(this.textBoxPlaces, "Кол машиномест");
            // 
            // comboBoxType
            // 
            this.comboBoxType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxType.AutoCompleteCustomSource.AddRange(new string[] {
            "Подземная",
            "Надземная"});
            this.comboBoxType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxType.FormattingEnabled = true;
            this.comboBoxType.Location = new System.Drawing.Point(101, 85);
            this.comboBoxType.Name = "comboBoxType";
            this.comboBoxType.Size = new System.Drawing.Size(131, 21);
            this.comboBoxType.TabIndex = 4;
            this.toolTip1.SetToolTip(this.comboBoxType, "Тип парковки");
            this.comboBoxType.SelectedIndexChanged += new System.EventHandler(this.comboBoxType_SelectedIndexChanged);
            // 
            // textBoxPlaceArea
            // 
            this.textBoxPlaceArea.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPlaceArea.Location = new System.Drawing.Point(247, 85);
            this.textBoxPlaceArea.Name = "textBoxPlaceArea";
            this.textBoxPlaceArea.ReadOnly = true;
            this.textBoxPlaceArea.Size = new System.Drawing.Size(39, 20);
            this.textBoxPlaceArea.TabIndex = 2;
            this.textBoxPlaceArea.Text = "40";
            this.toolTip1.SetToolTip(this.textBoxPlaceArea, "Площадь одного машиноместа, м2");
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(20, 88);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(26, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Тип";
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(292, 88);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(21, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "м2";
            // 
            // FormAreaParking
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(321, 183);
            this.Controls.Add(this.comboBoxType);
            this.Controls.Add(this.buttonInsertText);
            this.Controls.Add(this.textBoxPlaces);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxPlaceArea);
            this.Controls.Add(this.textBoxArea);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxFloors);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.labelFloors);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormAreaParking";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Свободная парковка";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelFloors;
        private System.Windows.Forms.TextBox textBoxFloors;
        private System.Windows.Forms.Button buttonInsertText;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxArea;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxPlaces;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBoxType;
        private System.Windows.Forms.TextBox textBoxPlaceArea;
        private System.Windows.Forms.Label label4;
    }
}