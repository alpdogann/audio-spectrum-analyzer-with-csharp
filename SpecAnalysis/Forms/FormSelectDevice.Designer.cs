namespace SpecAnalysis
{
    partial class FormSelectDevice
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
            this.cmbAsioDevices = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.labelAudioDrivers = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cmbAsioDevices
            // 
            this.cmbAsioDevices.FormattingEnabled = true;
            this.cmbAsioDevices.Location = new System.Drawing.Point(129, 103);
            this.cmbAsioDevices.Name = "cmbAsioDevices";
            this.cmbAsioDevices.Size = new System.Drawing.Size(186, 24);
            this.cmbAsioDevices.TabIndex = 0;
            this.cmbAsioDevices.SelectedIndexChanged += new System.EventHandler(this.cmbAsioDevices_SelectedIndexChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(172, 151);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(88, 31);
            this.button1.TabIndex = 1;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // labelAudioDrivers
            // 
            this.labelAudioDrivers.AutoSize = true;
            this.labelAudioDrivers.Location = new System.Drawing.Point(147, 74);
            this.labelAudioDrivers.Name = "labelAudioDrivers";
            this.labelAudioDrivers.Size = new System.Drawing.Size(149, 17);
            this.labelAudioDrivers.TabIndex = 2;
            this.labelAudioDrivers.Text = "Installed Audio Drivers";
            // 
            // FormSelectDevice
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(456, 261);
            this.Controls.Add(this.labelAudioDrivers);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.cmbAsioDevices);
            this.Name = "FormSelectDevice";
            this.Text = "Select an Audio Device";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbAsioDevices;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label labelAudioDrivers;
    }
}