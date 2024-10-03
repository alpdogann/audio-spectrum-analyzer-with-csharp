using CSharpDSPLibrary.Wave;
using System;
using System.Windows.Forms;


namespace SpecAnalysis
{
    partial class SpectrumAnalyzer
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
            this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown3 = new System.Windows.Forms.NumericUpDown();
            this.label20 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.btnSavePoints = new System.Windows.Forms.Button();
            this.txtSlope = new System.Windows.Forms.TextBox();
            this.button3 = new System.Windows.Forms.Button();
            this.checkBoxPeak = new System.Windows.Forms.CheckBox();
            this.vScrollBar1 = new System.Windows.Forms.VScrollBar();
            this.vScrollBar2 = new System.Windows.Forms.VScrollBar();
            this.btnPeakReset = new System.Windows.Forms.Button();
            this.btnCaptureSpectrum = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.button5 = new System.Windows.Forms.Button();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.button6 = new System.Windows.Forms.Button();
            this.btnSavePeaks = new System.Windows.Forms.Button();
            this.groupBoxEnvelope = new System.Windows.Forms.GroupBox();
            this.groupBoxFilters = new System.Windows.Forms.GroupBox();
            this.checkBoxApplyFilters = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBoxPoints = new System.Windows.Forms.GroupBox();
            this.lblWavefile = new System.Windows.Forms.Label();
            this.button4 = new System.Windows.Forms.Button();
            this.btnCaptureWaveform = new SpecAnalysis.UI_Tools.RoundButton();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).BeginInit();
            this.groupBoxEnvelope.SuspendLayout();
            this.groupBoxFilters.SuspendLayout();
            this.groupBoxPoints.SuspendLayout();
            this.SuspendLayout();
            // 
            // numericUpDown2
            // 
            this.numericUpDown2.BackColor = System.Drawing.Color.LightGoldenrodYellow;
            this.numericUpDown2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.numericUpDown2.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDown2.Location = new System.Drawing.Point(104, 30);
            this.numericUpDown2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.numericUpDown2.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.numericUpDown2.Name = "numericUpDown2";
            this.numericUpDown2.Size = new System.Drawing.Size(120, 18);
            this.numericUpDown2.TabIndex = 17;
            this.numericUpDown2.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDown2.ValueChanged += new System.EventHandler(this.numericUpDown2_ValueChanged);
            // 
            // numericUpDown3
            // 
            this.numericUpDown3.BackColor = System.Drawing.Color.LightGoldenrodYellow;
            this.numericUpDown3.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDown3.Location = new System.Drawing.Point(379, 30);
            this.numericUpDown3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.numericUpDown3.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.numericUpDown3.Name = "numericUpDown3";
            this.numericUpDown3.Size = new System.Drawing.Size(120, 22);
            this.numericUpDown3.TabIndex = 18;
            this.numericUpDown3.Value = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.numericUpDown3.ValueChanged += new System.EventHandler(this.numericUpDown3_ValueChanged);
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.ForeColor = System.Drawing.Color.Silver;
            this.label20.Location = new System.Drawing.Point(5, 32);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(83, 17);
            this.label20.TabIndex = 19;
            this.label20.Text = " Attack (ms)";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.ForeColor = System.Drawing.Color.Silver;
            this.label21.Location = new System.Drawing.Point(277, 32);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(92, 17);
            this.label21.TabIndex = 20;
            this.label21.Text = "Release (ms)";
            // 
            // comboBox1
            // 
            this.comboBox1.BackColor = System.Drawing.Color.LightGoldenrodYellow;
            this.comboBox1.Cursor = System.Windows.Forms.Cursors.Default;
            this.comboBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "X Log & Y Log",
            "X Linear & Y Log"});
            this.comboBox1.Location = new System.Drawing.Point(281, 249);
            this.comboBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(143, 24);
            this.comboBox1.TabIndex = 29;
            this.comboBox1.Text = "X Log & Y Log";
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // treeView1
            // 
            this.treeView1.BackColor = System.Drawing.Color.SlateGray;
            this.treeView1.Location = new System.Drawing.Point(7, 50);
            this.treeView1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(216, 111);
            this.treeView1.TabIndex = 31;
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            // 
            // button1
            // 
            this.button1.Enabled = false;
            this.button1.Location = new System.Drawing.Point(327, 46);
            this.button1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(216, 23);
            this.button1.TabIndex = 32;
            this.button1.Text = "Add Points";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.Maroon;
            this.button2.Enabled = false;
            this.button2.FlatAppearance.BorderColor = System.Drawing.Color.Maroon;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.button2.ForeColor = System.Drawing.Color.Silver;
            this.button2.Location = new System.Drawing.Point(327, 80);
            this.button2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(216, 23);
            this.button2.TabIndex = 33;
            this.button2.Text = "Delete Point";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // btnSavePoints
            // 
            this.btnSavePoints.Enabled = false;
            this.btnSavePoints.Location = new System.Drawing.Point(327, 110);
            this.btnSavePoints.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnSavePoints.Name = "btnSavePoints";
            this.btnSavePoints.Size = new System.Drawing.Size(216, 23);
            this.btnSavePoints.TabIndex = 34;
            this.btnSavePoints.Text = "Save Points";
            this.btnSavePoints.UseVisualStyleBackColor = true;
            this.btnSavePoints.Click += new System.EventHandler(this.btnSavePoints_Click);
            // 
            // txtSlope
            // 
            this.txtSlope.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtSlope.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.txtSlope.Location = new System.Drawing.Point(509, 252);
            this.txtSlope.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtSlope.Name = "txtSlope";
            this.txtSlope.ReadOnly = true;
            this.txtSlope.Size = new System.Drawing.Size(141, 16);
            this.txtSlope.TabIndex = 35;
            // 
            // button3
            // 
            this.button3.BackColor = System.Drawing.Color.Maroon;
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button3.ForeColor = System.Drawing.Color.Silver;
            this.button3.Location = new System.Drawing.Point(327, 138);
            this.button3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(216, 23);
            this.button3.TabIndex = 41;
            this.button3.Text = "Delete Last Point";
            this.button3.UseVisualStyleBackColor = false;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // checkBoxPeak
            // 
            this.checkBoxPeak.AutoSize = true;
            this.checkBoxPeak.ForeColor = System.Drawing.Color.Silver;
            this.checkBoxPeak.Location = new System.Drawing.Point(923, 604);
            this.checkBoxPeak.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.checkBoxPeak.Name = "checkBoxPeak";
            this.checkBoxPeak.Size = new System.Drawing.Size(62, 21);
            this.checkBoxPeak.TabIndex = 42;
            this.checkBoxPeak.Text = "Peak";
            this.checkBoxPeak.UseVisualStyleBackColor = true;
            this.checkBoxPeak.Visible = false;
            this.checkBoxPeak.CheckedChanged += new System.EventHandler(this.checkBoxPeak_CheckedChanged);
            // 
            // vScrollBar1
            // 
            this.vScrollBar1.LargeChange = 20;
            this.vScrollBar1.Location = new System.Drawing.Point(13, 240);
            this.vScrollBar1.Maximum = 0;
            this.vScrollBar1.Minimum = -120;
            this.vScrollBar1.Name = "vScrollBar1";
            this.vScrollBar1.Size = new System.Drawing.Size(10, 345);
            this.vScrollBar1.SmallChange = 10;
            this.vScrollBar1.TabIndex = 43;
            this.vScrollBar1.Value = -120;
            this.vScrollBar1.Scroll += new System.Windows.Forms.ScrollEventHandler(this.vScrollBar1_Scroll);
            // 
            // vScrollBar2
            // 
            this.vScrollBar2.Location = new System.Drawing.Point(1691, 249);
            this.vScrollBar2.Maximum = 50;
            this.vScrollBar2.Minimum = 10;
            this.vScrollBar2.Name = "vScrollBar2";
            this.vScrollBar2.Size = new System.Drawing.Size(10, 335);
            this.vScrollBar2.SmallChange = 10;
            this.vScrollBar2.TabIndex = 44;
            this.vScrollBar2.Value = 10;
            this.vScrollBar2.Scroll += new System.Windows.Forms.ScrollEventHandler(this.vScrollBar2_Scroll);
            // 
            // btnPeakReset
            // 
            this.btnPeakReset.Location = new System.Drawing.Point(989, 594);
            this.btnPeakReset.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnPeakReset.Name = "btnPeakReset";
            this.btnPeakReset.Size = new System.Drawing.Size(107, 36);
            this.btnPeakReset.TabIndex = 45;
            this.btnPeakReset.Text = "Reset Peak";
            this.btnPeakReset.UseVisualStyleBackColor = true;
            this.btnPeakReset.Visible = false;
            this.btnPeakReset.Click += new System.EventHandler(this.btnPeakReset_Click);
            // 
            // btnCaptureSpectrum
            // 
            this.btnCaptureSpectrum.BackColor = System.Drawing.SystemColors.Control;
            this.btnCaptureSpectrum.Location = new System.Drawing.Point(1119, 594);
            this.btnCaptureSpectrum.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnCaptureSpectrum.Name = "btnCaptureSpectrum";
            this.btnCaptureSpectrum.Size = new System.Drawing.Size(136, 36);
            this.btnCaptureSpectrum.TabIndex = 46;
            this.btnCaptureSpectrum.Text = "Capture Spectrum";
            this.btnCaptureSpectrum.UseVisualStyleBackColor = true;
            this.btnCaptureSpectrum.Click += new System.EventHandler(this.button4_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.DimGray;
            this.panel1.Location = new System.Drawing.Point(384, 19);
            this.panel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1128, 112);
            this.panel1.TabIndex = 68;
            // 
            // button5
            // 
            this.button5.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.button5.Location = new System.Drawing.Point(294, 45);
            this.button5.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(80, 29);
            this.button5.TabIndex = 69;
            this.button5.Text = "+";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // listBox1
            // 
            this.listBox1.BackColor = System.Drawing.Color.SlateGray;
            this.listBox1.Enabled = false;
            this.listBox1.ForeColor = System.Drawing.Color.Silver;
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 16;
            this.listBox1.Location = new System.Drawing.Point(5, 45);
            this.listBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(283, 68);
            this.listBox1.TabIndex = 70;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // button6
            // 
            this.button6.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.button6.Location = new System.Drawing.Point(294, 78);
            this.button6.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(80, 35);
            this.button6.TabIndex = 71;
            this.button6.Text = "Save";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // btnSavePeaks
            // 
            this.btnSavePeaks.Location = new System.Drawing.Point(795, 594);
            this.btnSavePeaks.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnSavePeaks.Name = "btnSavePeaks";
            this.btnSavePeaks.Size = new System.Drawing.Size(107, 36);
            this.btnSavePeaks.TabIndex = 72;
            this.btnSavePeaks.Text = "Save Peaks";
            this.btnSavePeaks.UseVisualStyleBackColor = true;
            this.btnSavePeaks.Visible = false;
            this.btnSavePeaks.Click += new System.EventHandler(this.btnSavePeaks_Click);
            // 
            // groupBoxEnvelope
            // 
            this.groupBoxEnvelope.BackColor = System.Drawing.Color.DimGray;
            this.groupBoxEnvelope.Controls.Add(this.label20);
            this.groupBoxEnvelope.Controls.Add(this.numericUpDown2);
            this.groupBoxEnvelope.Controls.Add(this.label21);
            this.groupBoxEnvelope.Controls.Add(this.numericUpDown3);
            this.groupBoxEnvelope.Location = new System.Drawing.Point(20, 633);
            this.groupBoxEnvelope.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBoxEnvelope.Name = "groupBoxEnvelope";
            this.groupBoxEnvelope.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBoxEnvelope.Size = new System.Drawing.Size(1681, 62);
            this.groupBoxEnvelope.TabIndex = 79;
            this.groupBoxEnvelope.TabStop = false;
            this.groupBoxEnvelope.Text = "Envelope Follower";
            // 
            // groupBoxFilters
            // 
            this.groupBoxFilters.BackColor = System.Drawing.Color.DimGray;
            this.groupBoxFilters.Controls.Add(this.checkBoxApplyFilters);
            this.groupBoxFilters.Controls.Add(this.listBox1);
            this.groupBoxFilters.Controls.Add(this.button5);
            this.groupBoxFilters.Controls.Add(this.button6);
            this.groupBoxFilters.Controls.Add(this.panel1);
            this.groupBoxFilters.ForeColor = System.Drawing.Color.Silver;
            this.groupBoxFilters.Location = new System.Drawing.Point(3, 699);
            this.groupBoxFilters.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBoxFilters.Name = "groupBoxFilters";
            this.groupBoxFilters.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBoxFilters.Size = new System.Drawing.Size(1698, 141);
            this.groupBoxFilters.TabIndex = 80;
            this.groupBoxFilters.TabStop = false;
            this.groupBoxFilters.Text = "Filters";
            // 
            // checkBoxApplyFilters
            // 
            this.checkBoxApplyFilters.AutoSize = true;
            this.checkBoxApplyFilters.ForeColor = System.Drawing.Color.Silver;
            this.checkBoxApplyFilters.Location = new System.Drawing.Point(88, 19);
            this.checkBoxApplyFilters.Name = "checkBoxApplyFilters";
            this.checkBoxApplyFilters.Size = new System.Drawing.Size(290, 21);
            this.checkBoxApplyFilters.TabIndex = 72;
            this.checkBoxApplyFilters.Text = "Apply Filters(Disables Envelope Follower)";
            this.checkBoxApplyFilters.UseVisualStyleBackColor = true;
            this.checkBoxApplyFilters.CheckedChanged += new System.EventHandler(this.checkBoxApplyFilters_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Location = new System.Drawing.Point(461, 1017);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox1.Size = new System.Drawing.Size(8, 7);
            this.groupBox1.TabIndex = 81;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "groupBox1";
            // 
            // groupBoxPoints
            // 
            this.groupBoxPoints.BackColor = System.Drawing.Color.Black;
            this.groupBoxPoints.Controls.Add(this.treeView1);
            this.groupBoxPoints.Controls.Add(this.button1);
            this.groupBoxPoints.Controls.Add(this.button2);
            this.groupBoxPoints.Controls.Add(this.btnSavePoints);
            this.groupBoxPoints.Controls.Add(this.button3);
            this.groupBoxPoints.ForeColor = System.Drawing.Color.Silver;
            this.groupBoxPoints.Location = new System.Drawing.Point(20, 48);
            this.groupBoxPoints.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBoxPoints.Name = "groupBoxPoints";
            this.groupBoxPoints.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBoxPoints.Size = new System.Drawing.Size(763, 172);
            this.groupBoxPoints.TabIndex = 82;
            this.groupBoxPoints.TabStop = false;
            this.groupBoxPoints.Text = "Marked Points (Double Right Click)";
            // 
            // lblWavefile
            // 
            this.lblWavefile.AutoSize = true;
            this.lblWavefile.ForeColor = System.Drawing.Color.Silver;
            this.lblWavefile.Location = new System.Drawing.Point(947, 39);
            this.lblWavefile.Name = "lblWavefile";
            this.lblWavefile.Size = new System.Drawing.Size(79, 17);
            this.lblWavefile.TabIndex = 83;
            this.lblWavefile.Text = "Wave Input";
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(463, 594);
            this.button4.Margin = new System.Windows.Forms.Padding(4);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(100, 36);
            this.button4.TabIndex = 90;
            this.button4.Text = "1stHarmonic";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Visible = false;
            this.button4.Click += new System.EventHandler(this.button4_Click_1);
            // 
            // btnCaptureWaveform
            // 
            this.btnCaptureWaveform.BackColor = System.Drawing.SystemColors.Control;
            this.btnCaptureWaveform.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnCaptureWaveform.Location = new System.Drawing.Point(795, 79);
            this.btnCaptureWaveform.Name = "btnCaptureWaveform";
            this.btnCaptureWaveform.Size = new System.Drawing.Size(131, 53);
            this.btnCaptureWaveform.TabIndex = 91;
            this.btnCaptureWaveform.Text = "Capture Waveform";
            this.btnCaptureWaveform.UseVisualStyleBackColor = true;
            this.btnCaptureWaveform.Click += new System.EventHandler(this.btnCaptureWaveform_Click_1);
            // 
            // SpectrumAnalyzer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.btnCaptureWaveform);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.btnCaptureSpectrum);
            this.Controls.Add(this.lblWavefile);
            this.Controls.Add(this.groupBoxPoints);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBoxFilters);
            this.Controls.Add(this.groupBoxEnvelope);
            this.Controls.Add(this.btnSavePeaks);
            this.Controls.Add(this.btnPeakReset);
            this.Controls.Add(this.vScrollBar2);
            this.Controls.Add(this.vScrollBar1);
            this.Controls.Add(this.checkBoxPeak);
            this.Controls.Add(this.txtSlope);
            this.Controls.Add(this.comboBox1);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "SpectrumAnalyzer";
            this.Size = new System.Drawing.Size(1708, 852);
            this.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.SpectrumAnalyzer_MouseDoubleClick);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.SpectrumAnalyzer_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SpectrumAnalyzer_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.SpectrumAnalyzer_MouseUp);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).EndInit();
            this.groupBoxEnvelope.ResumeLayout(false);
            this.groupBoxEnvelope.PerformLayout();
            this.groupBoxFilters.ResumeLayout(false);
            this.groupBoxFilters.PerformLayout();
            this.groupBoxPoints.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private NumericUpDown numericUpDown2;
        private NumericUpDown numericUpDown3;
        private Label label20;
        private Label label21;
        private ComboBox comboBox1;
        private TreeView treeView1;
        private Button button1;
        private Button button2;
        private Button btnSavePoints;
        private TextBox txtSlope;
        private Button button3;
        private CheckBox checkBoxPeak;
        private VScrollBar vScrollBar1;
        private VScrollBar vScrollBar2;
        private Button btnPeakReset;
        private Button btnCaptureSpectrum;
        private Panel panel1;
        private Button button5;
        private ListBox listBox1;
        private Button button6;
        private Button btnSavePeaks;
        private GroupBox groupBoxEnvelope;
        private GroupBox groupBoxFilters;
        private GroupBox groupBox1;
        private GroupBox groupBoxPoints;
        private Label lblWavefile;
        private Button button4;
        private CheckBox checkBoxApplyFilters;
        private UI_Tools.RoundButton btnCaptureWaveform;
#pragma warning disable CS0436 // Type conflicts with imported type
#pragma warning restore CS0436 // Type conflicts with imported type
    }
}

