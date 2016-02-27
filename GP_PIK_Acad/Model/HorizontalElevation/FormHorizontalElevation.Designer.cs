namespace GP_PIK_Acad.Model.HorizontalElevation
{
   partial class FormHorizontalElevation
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
         this.label1 = new System.Windows.Forms.Label();
         this.buttonStart = new System.Windows.Forms.Button();
         this.label2 = new System.Windows.Forms.Label();
         this.textBoxStartElevation = new System.Windows.Forms.TextBox();
         this.textBoxStepElevation = new System.Windows.Forms.TextBox();
         this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
         this.buttonoptions = new System.Windows.Forms.Button();
         ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
         this.SuspendLayout();
         // 
         // label1
         // 
         this.label1.AutoSize = true;
         this.label1.Location = new System.Drawing.Point(21, 15);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(106, 13);
         this.label1.TabIndex = 0;
         this.label1.Text = "Стартовый уровень";
         // 
         // buttonStart
         // 
         this.buttonStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.buttonStart.DialogResult = System.Windows.Forms.DialogResult.OK;
         this.buttonStart.Location = new System.Drawing.Point(173, 81);
         this.buttonStart.Name = "buttonStart";
         this.buttonStart.Size = new System.Drawing.Size(75, 23);
         this.buttonStart.TabIndex = 3;
         this.buttonStart.Text = "Старт";
         this.buttonStart.UseVisualStyleBackColor = true;
         this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
         // 
         // label2
         // 
         this.label2.AutoSize = true;
         this.label2.Location = new System.Drawing.Point(21, 44);
         this.label2.Name = "label2";
         this.label2.Size = new System.Drawing.Size(86, 13);
         this.label2.TabIndex = 0;
         this.label2.Text = "Шаг изменения";
         // 
         // textBoxStartElevation
         // 
         this.textBoxStartElevation.Location = new System.Drawing.Point(133, 12);
         this.textBoxStartElevation.Name = "textBoxStartElevation";
         this.textBoxStartElevation.Size = new System.Drawing.Size(100, 20);
         this.textBoxStartElevation.TabIndex = 4;
         // 
         // textBoxStepElevation
         // 
         this.textBoxStepElevation.Location = new System.Drawing.Point(133, 41);
         this.textBoxStepElevation.Name = "textBoxStepElevation";
         this.textBoxStepElevation.Size = new System.Drawing.Size(100, 20);
         this.textBoxStepElevation.TabIndex = 4;
         // 
         // errorProvider1
         // 
         this.errorProvider1.ContainerControl = this;
         // 
         // buttonoptions
         // 
         this.buttonoptions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.buttonoptions.BackgroundImage = global::GP_PIK_Acad.Properties.Resources.options;
         this.buttonoptions.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
         this.buttonoptions.Location = new System.Drawing.Point(12, 81);
         this.buttonoptions.Name = "buttonoptions";
         this.buttonoptions.Size = new System.Drawing.Size(25, 25);
         this.buttonoptions.TabIndex = 5;
         this.buttonoptions.UseVisualStyleBackColor = true;
         this.buttonoptions.Click += new System.EventHandler(this.buttonoptions_Click);
         // 
         // FormHorizontalElevation
         // 
         this.AcceptButton = this.buttonStart;
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(260, 116);
         this.Controls.Add(this.buttonoptions);
         this.Controls.Add(this.textBoxStepElevation);
         this.Controls.Add(this.textBoxStartElevation);
         this.Controls.Add(this.buttonStart);
         this.Controls.Add(this.label2);
         this.Controls.Add(this.label1);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "FormHorizontalElevation";
         this.ShowIcon = false;
         this.ShowInTaskbar = false;
         this.Text = "Изменение уровня горизонталей";
         ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.Button buttonStart;
      private System.Windows.Forms.Label label2;
      private System.Windows.Forms.TextBox textBoxStartElevation;
      private System.Windows.Forms.TextBox textBoxStepElevation;
      private System.Windows.Forms.ErrorProvider errorProvider1;
      private System.Windows.Forms.Button buttonoptions;
   }
}