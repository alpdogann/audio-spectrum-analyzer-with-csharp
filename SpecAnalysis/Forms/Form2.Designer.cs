namespace SpecAnalysis
{
    partial class Form2
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
            this.cmbWaveforms = new System.Windows.Forms.ComboBox();
            this.lblSineFrequency = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown3 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown4 = new System.Windows.Forms.NumericUpDown();
            this.lblStart = new System.Windows.Forms.Label();
            this.lblEnd = new System.Windows.Forms.Label();
            this.lblDuration = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbWindow = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.lblMultidrive = new System.Windows.Forms.Label();
            this.numMultidrive = new System.Windows.Forms.NumericUpDown();
            this.txtFileName = new System.Windows.Forms.TextBox();
            this.spectrumAnalyzer1 = new SpecAnalysis.SpectrumAnalyzer();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown4)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMultidrive)).BeginInit();
            this.SuspendLayout();
            // 
            // cmbWaveforms
            // 
            this.cmbWaveforms.BackColor = System.Drawing.Color.LightGoldenrodYellow;
            this.cmbWaveforms.FormattingEnabled = true;
            this.cmbWaveforms.Items.AddRange(new object[] {
            "White Noise",
            "Sine Wave",
            "Linear Sine Sweep",
            "Triangle",
            "Square",
            "Sawtooth",
            "Clipped Sine",
            "Half-wave Rectified Sine",
            "Full-wave Rectified Sine"});
            this.cmbWaveforms.Location = new System.Drawing.Point(170, 239);
            this.cmbWaveforms.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cmbWaveforms.Name = "cmbWaveforms";
            this.cmbWaveforms.Size = new System.Drawing.Size(121, 24);
            this.cmbWaveforms.TabIndex = 1;
            this.cmbWaveforms.Text = "White Noise";
            this.cmbWaveforms.SelectedIndexChanged += new System.EventHandler(this.cmbWaveforms_SelectedIndexChanged);
            // 
            // lblSineFrequency
            // 
            this.lblSineFrequency.AutoSize = true;
            this.lblSineFrequency.Location = new System.Drawing.Point(15, 512);
            this.lblSineFrequency.Name = "lblSineFrequency";
            this.lblSineFrequency.Size = new System.Drawing.Size(75, 17);
            this.lblSineFrequency.TabIndex = 3;
            this.lblSineFrequency.Text = "Frequency";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.BackColor = System.Drawing.Color.LightGoldenrodYellow;
            this.numericUpDown1.Enabled = false;
            this.numericUpDown1.Increment = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDown1.Location = new System.Drawing.Point(187, 512);
            this.numericUpDown1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            23000,
            0,
            0,
            0});
            this.numericUpDown1.Minimum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(121, 22);
            this.numericUpDown1.TabIndex = 4;
            this.numericUpDown1.Value = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDown1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // numericUpDown2
            // 
            this.numericUpDown2.BackColor = System.Drawing.Color.LightGoldenrodYellow;
            this.numericUpDown2.Enabled = false;
            this.numericUpDown2.Increment = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDown2.Location = new System.Drawing.Point(187, 555);
            this.numericUpDown2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.numericUpDown2.Maximum = new decimal(new int[] {
            900,
            0,
            0,
            0});
            this.numericUpDown2.Minimum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.numericUpDown2.Name = "numericUpDown2";
            this.numericUpDown2.Size = new System.Drawing.Size(120, 22);
            this.numericUpDown2.TabIndex = 5;
            this.numericUpDown2.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.numericUpDown2.ValueChanged += new System.EventHandler(this.numericUpDown2_ValueChanged);
            // 
            // numericUpDown3
            // 
            this.numericUpDown3.BackColor = System.Drawing.Color.LightGoldenrodYellow;
            this.numericUpDown3.Enabled = false;
            this.numericUpDown3.Increment = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDown3.Location = new System.Drawing.Point(187, 595);
            this.numericUpDown3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.numericUpDown3.Maximum = new decimal(new int[] {
            20000,
            0,
            0,
            0});
            this.numericUpDown3.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDown3.Name = "numericUpDown3";
            this.numericUpDown3.Size = new System.Drawing.Size(120, 22);
            this.numericUpDown3.TabIndex = 6;
            this.numericUpDown3.Value = new decimal(new int[] {
            20000,
            0,
            0,
            0});
            this.numericUpDown3.ValueChanged += new System.EventHandler(this.numericUpDown3_ValueChanged);
            // 
            // numericUpDown4
            // 
            this.numericUpDown4.BackColor = System.Drawing.Color.LightGoldenrodYellow;
            this.numericUpDown4.Enabled = false;
            this.numericUpDown4.Location = new System.Drawing.Point(187, 633);
            this.numericUpDown4.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.numericUpDown4.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.numericUpDown4.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericUpDown4.Name = "numericUpDown4";
            this.numericUpDown4.Size = new System.Drawing.Size(120, 22);
            this.numericUpDown4.TabIndex = 7;
            this.numericUpDown4.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDown4.ValueChanged += new System.EventHandler(this.numericUpDown4_ValueChanged);
            // 
            // lblStart
            // 
            this.lblStart.AutoSize = true;
            this.lblStart.Location = new System.Drawing.Point(13, 557);
            this.lblStart.Name = "lblStart";
            this.lblStart.Size = new System.Drawing.Size(155, 17);
            this.lblStart.TabIndex = 8;
            this.lblStart.Text = "Sweep Start Frequency";
            // 
            // lblEnd
            // 
            this.lblEnd.AutoSize = true;
            this.lblEnd.Location = new System.Drawing.Point(13, 597);
            this.lblEnd.Name = "lblEnd";
            this.lblEnd.Size = new System.Drawing.Size(150, 17);
            this.lblEnd.TabIndex = 9;
            this.lblEnd.Text = "Sweep End Frequency";
            // 
            // lblDuration
            // 
            this.lblDuration.AutoSize = true;
            this.lblDuration.Location = new System.Drawing.Point(13, 636);
            this.lblDuration.Name = "lblDuration";
            this.lblDuration.Size = new System.Drawing.Size(108, 17);
            this.lblDuration.TabIndex = 10;
            this.lblDuration.Text = "Sweep Duration";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 295);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 17);
            this.label1.TabIndex = 11;
            this.label1.Text = "Window Type";
            // 
            // cmbWindow
            // 
            this.cmbWindow.BackColor = System.Drawing.Color.LightGoldenrodYellow;
            this.cmbWindow.FormattingEnabled = true;
            this.cmbWindow.Items.AddRange(new object[] {
            "Hamming Window",
            "Hanning Window",
            "Blackman Window",
            "Blackman Harris Window",
            "Rectangular"});
            this.cmbWindow.Location = new System.Drawing.Point(170, 295);
            this.cmbWindow.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cmbWindow.Name = "cmbWindow";
            this.cmbWindow.Size = new System.Drawing.Size(137, 24);
            this.cmbWindow.TabIndex = 12;
            this.cmbWindow.Text = "Blackman Harris Window";
            this.cmbWindow.SelectedIndexChanged += new System.EventHandler(this.cmbWindow_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 235);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 17);
            this.label2.TabIndex = 13;
            this.label2.Text = "Wave Type";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(98, 147);
            this.button1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(91, 32);
            this.button1.TabIndex = 14;
            this.button1.Text = "Record";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.AutoSize = true;
            this.groupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox1.BackColor = System.Drawing.Color.DimGray;
            this.groupBox1.Controls.Add(this.checkBox1);
            this.groupBox1.Controls.Add(this.lblMultidrive);
            this.groupBox1.Controls.Add(this.numMultidrive);
            this.groupBox1.Controls.Add(this.txtFileName);
            this.groupBox1.Controls.Add(this.lblSineFrequency);
            this.groupBox1.Controls.Add(this.numericUpDown4);
            this.groupBox1.Controls.Add(this.lblDuration);
            this.groupBox1.Controls.Add(this.numericUpDown3);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.numericUpDown2);
            this.groupBox1.Controls.Add(this.lblEnd);
            this.groupBox1.Controls.Add(this.numericUpDown1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.lblStart);
            this.groupBox1.Controls.Add(this.cmbWindow);
            this.groupBox1.Controls.Add(this.cmbWaveforms);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Right;
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.groupBox1.Location = new System.Drawing.Point(1775, 0);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox1.Size = new System.Drawing.Size(314, 880);
            this.groupBox1.TabIndex = 15;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Input Waveform";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(7, 432);
            this.checkBox1.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(63, 21);
            this.checkBox1.TabIndex = 27;
            this.checkBox1.Text = "Drive";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // lblMultidrive
            // 
            this.lblMultidrive.AutoSize = true;
            this.lblMultidrive.Location = new System.Drawing.Point(3, 403);
            this.lblMultidrive.Name = "lblMultidrive";
            this.lblMultidrive.Size = new System.Drawing.Size(90, 17);
            this.lblMultidrive.TabIndex = 26;
            this.lblMultidrive.Text = "MULTIDRIVE";
            // 
            // numMultidrive
            // 
            this.numMultidrive.BackColor = System.Drawing.Color.LightGoldenrodYellow;
            this.numMultidrive.Location = new System.Drawing.Point(186, 403);
            this.numMultidrive.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.numMultidrive.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numMultidrive.Name = "numMultidrive";
            this.numMultidrive.Size = new System.Drawing.Size(121, 22);
            this.numMultidrive.TabIndex = 25;
            this.numMultidrive.ValueChanged += new System.EventHandler(this.numMultidrive_ValueChanged);
            // 
            // txtFileName
            // 
            this.txtFileName.BackColor = System.Drawing.Color.LightGoldenrodYellow;
            this.txtFileName.Location = new System.Drawing.Point(42, 104);
            this.txtFileName.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtFileName.Name = "txtFileName";
            this.txtFileName.Size = new System.Drawing.Size(225, 22);
            this.txtFileName.TabIndex = 22;
            this.txtFileName.Text = "Record";
            // 
            // spectrumAnalyzer1
            // 
            this.spectrumAnalyzer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.spectrumAnalyzer1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.spectrumAnalyzer1.BackColor = System.Drawing.Color.Black;
            this.spectrumAnalyzer1.Location = new System.Drawing.Point(12, 11);
            this.spectrumAnalyzer1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.spectrumAnalyzer1.Name = "spectrumAnalyzer1";
            this.spectrumAnalyzer1.Size = new System.Drawing.Size(1757, 858);
            this.spectrumAnalyzer1.TabIndex = 16;
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(2089, 880);
            this.Controls.Add(this.spectrumAnalyzer1);
            this.Controls.Add(this.groupBox1);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "Form2";
            this.Text = "Spectrum Analyzer";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown4)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMultidrive)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        
        private System.Windows.Forms.ComboBox cmbWaveforms;
        private System.Windows.Forms.Label lblSineFrequency;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.NumericUpDown numericUpDown2;
        private System.Windows.Forms.NumericUpDown numericUpDown3;
        private System.Windows.Forms.NumericUpDown numericUpDown4;
        private System.Windows.Forms.Label lblStart;
        private System.Windows.Forms.Label lblEnd;
        private System.Windows.Forms.Label lblDuration;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbWindow;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtFileName;
        private System.Windows.Forms.Label lblMultidrive;
        private System.Windows.Forms.NumericUpDown numMultidrive;
        private System.Windows.Forms.CheckBox checkBox1;
        private SpectrumAnalyzer spectrumAnalyzer1;
    }
}