namespace SpecAnalysis
{
    partial class FilterPanelUC
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
            this.lblRes = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lblGain = new System.Windows.Forms.Label();
            this.filterPanel = new System.Windows.Forms.Panel();
            this.btnClear = new System.Windows.Forms.Button();
            this.numRes = new System.Windows.Forms.NumericUpDown();
            this.numGain = new System.Windows.Forms.NumericUpDown();
            this.filterLabel = new System.Windows.Forms.Label();
            this.numCutoff = new System.Windows.Forms.NumericUpDown();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.comboBox2 = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.filterPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numRes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGain)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCutoff)).BeginInit();
            this.SuspendLayout();
            // 
            // lblRes
            // 
            this.lblRes.AutoSize = true;
            this.lblRes.Location = new System.Drawing.Point(416, 74);
            this.lblRes.Name = "lblRes";
            this.lblRes.Size = new System.Drawing.Size(80, 17);
            this.lblRes.TabIndex = 24;
            this.lblRes.Text = "Resonance";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(221, 74);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(45, 17);
            this.label7.TabIndex = 23;
            this.label7.Text = "Cutoff";
            // 
            // lblGain
            // 
            this.lblGain.AutoSize = true;
            this.lblGain.Location = new System.Drawing.Point(40, 74);
            this.lblGain.Name = "lblGain";
            this.lblGain.Size = new System.Drawing.Size(38, 17);
            this.lblGain.TabIndex = 22;
            this.lblGain.Text = "Gain";
            // 
            // filterPanel
            // 
            this.filterPanel.Controls.Add(this.btnClear);
            this.filterPanel.Controls.Add(this.numRes);
            this.filterPanel.Controls.Add(this.lblRes);
            this.filterPanel.Controls.Add(this.numGain);
            this.filterPanel.Controls.Add(this.label7);
            this.filterPanel.Controls.Add(this.filterLabel);
            this.filterPanel.Controls.Add(this.lblGain);
            this.filterPanel.Controls.Add(this.numCutoff);
            this.filterPanel.Enabled = false;
            this.filterPanel.Location = new System.Drawing.Point(225, 15);
            this.filterPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.filterPanel.Name = "filterPanel";
            this.filterPanel.Size = new System.Drawing.Size(627, 159);
            this.filterPanel.TabIndex = 21;
            // 
            // btnClear
            // 
            this.btnClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClear.Location = new System.Drawing.Point(19, 2);
            this.btnClear.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(87, 28);
            this.btnClear.TabIndex = 51;
            this.btnClear.Text = "Reset";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // numRes
            // 
            this.numRes.Location = new System.Drawing.Point(523, 71);
            this.numRes.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.numRes.Maximum = new decimal(new int[] {
            127,
            0,
            0,
            0});
            this.numRes.Name = "numRes";
            this.numRes.Size = new System.Drawing.Size(87, 22);
            this.numRes.TabIndex = 50;
            this.numRes.ValueChanged += new System.EventHandler(this.numRes_ValueChanged);
            // 
            // numGain
            // 
            this.numGain.Location = new System.Drawing.Point(107, 71);
            this.numGain.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.numGain.Maximum = new decimal(new int[] {
            127,
            0,
            0,
            0});
            this.numGain.Name = "numGain";
            this.numGain.Size = new System.Drawing.Size(87, 22);
            this.numGain.TabIndex = 49;
            this.numGain.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numGain.ValueChanged += new System.EventHandler(this.numGain_ValueChanged);
            // 
            // filterLabel
            // 
            this.filterLabel.AutoSize = true;
            this.filterLabel.Location = new System.Drawing.Point(15, 39);
            this.filterLabel.Name = "filterLabel";
            this.filterLabel.Size = new System.Drawing.Size(0, 17);
            this.filterLabel.TabIndex = 1;
            // 
            // numCutoff
            // 
            this.numCutoff.Increment = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.numCutoff.Location = new System.Drawing.Point(297, 71);
            this.numCutoff.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.numCutoff.Maximum = new decimal(new int[] {
            22000,
            0,
            0,
            0});
            this.numCutoff.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numCutoff.Name = "numCutoff";
            this.numCutoff.Size = new System.Drawing.Size(87, 22);
            this.numCutoff.TabIndex = 48;
            this.numCutoff.Value = new decimal(new int[] {
            1408,
            0,
            0,
            0});
            this.numCutoff.ValueChanged += new System.EventHandler(this.numCutoff_ValueChanged);
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "Lowpass",
            "Highpass",
            "Bandpass",
            "Bandstop",
            "LowShelf",
            "HighShelf",
            "Comb",
            "Peaking"});
            this.comboBox1.Location = new System.Drawing.Point(59, 22);
            this.comboBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(105, 24);
            this.comboBox1.TabIndex = 25;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // comboBox2
            // 
            this.comboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox2.Enabled = false;
            this.comboBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.comboBox2.FormattingEnabled = true;
            this.comboBox2.Items.AddRange(new object[] {
            "12 dB",
            "24 dB"});
            this.comboBox2.Location = new System.Drawing.Point(59, 68);
            this.comboBox2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.comboBox2.Name = "comboBox2";
            this.comboBox2.Size = new System.Drawing.Size(105, 24);
            this.comboBox2.TabIndex = 26;
            this.comboBox2.SelectedIndexChanged += new System.EventHandler(this.comboBox2_SelectedIndexChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(52, 112);
            this.button1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(112, 37);
            this.button1.TabIndex = 27;
            this.button1.Text = "Change Color";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // FilterPanelUC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.button1);
            this.Controls.Add(this.comboBox2);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.filterPanel);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "FilterPanelUC";
            this.Size = new System.Drawing.Size(880, 186);
            this.filterPanel.ResumeLayout(false);
            this.filterPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numRes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGain)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCutoff)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblRes;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lblGain;
        private System.Windows.Forms.Panel filterPanel;
        private System.Windows.Forms.NumericUpDown numRes;
        private System.Windows.Forms.NumericUpDown numGain;
        private System.Windows.Forms.Label filterLabel;
        private System.Windows.Forms.NumericUpDown numCutoff;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.ComboBox comboBox2;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button button1;
    }
}