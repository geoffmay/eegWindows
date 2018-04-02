namespace Analysis
{
    partial class FormTrace
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
            this.pictureBoxEeg = new System.Windows.Forms.PictureBox();
            this.hScrollBarWindow = new System.Windows.Forms.HScrollBar();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.selectionLabelsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.analysisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fourierAttributesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.numericUpDownGain = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBoxAutoGain = new System.Windows.Forms.CheckBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.labelStatus = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxEeg)).BeginInit();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownGain)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBoxEeg
            // 
            this.pictureBoxEeg.Location = new System.Drawing.Point(100, 83);
            this.pictureBoxEeg.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pictureBoxEeg.Name = "pictureBoxEeg";
            this.pictureBoxEeg.Size = new System.Drawing.Size(1036, 512);
            this.pictureBoxEeg.TabIndex = 0;
            this.pictureBoxEeg.TabStop = false;
            // 
            // hScrollBarWindow
            // 
            this.hScrollBarWindow.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hScrollBarWindow.Location = new System.Drawing.Point(100, 645);
            this.hScrollBarWindow.Name = "hScrollBarWindow";
            this.hScrollBarWindow.Size = new System.Drawing.Size(1041, 20);
            this.hScrollBarWindow.TabIndex = 1;
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.selectionToolStripMenuItem,
            this.analysisToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(9, 3, 0, 3);
            this.menuStrip1.Size = new System.Drawing.Size(1155, 35);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openFileToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(50, 29);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openFileToolStripMenuItem
            // 
            this.openFileToolStripMenuItem.Name = "openFileToolStripMenuItem";
            this.openFileToolStripMenuItem.Size = new System.Drawing.Size(172, 30);
            this.openFileToolStripMenuItem.Text = "Open File";
            this.openFileToolStripMenuItem.Click += new System.EventHandler(this.openFileToolStripMenuItem_Click);
            // 
            // selectionToolStripMenuItem
            // 
            this.selectionToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.selectionLabelsToolStripMenuItem});
            this.selectionToolStripMenuItem.Name = "selectionToolStripMenuItem";
            this.selectionToolStripMenuItem.Size = new System.Drawing.Size(95, 29);
            this.selectionToolStripMenuItem.Text = "Selection";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(231, 30);
            this.toolStripMenuItem1.Text = "Display Channels";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.displayChannelsToolStripMenuItem_Click);
            // 
            // selectionLabelsToolStripMenuItem
            // 
            this.selectionLabelsToolStripMenuItem.Name = "selectionLabelsToolStripMenuItem";
            this.selectionLabelsToolStripMenuItem.Size = new System.Drawing.Size(231, 30);
            this.selectionLabelsToolStripMenuItem.Text = "Selection Labels";
            // 
            // analysisToolStripMenuItem
            // 
            this.analysisToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fourierAttributesToolStripMenuItem});
            this.analysisToolStripMenuItem.Name = "analysisToolStripMenuItem";
            this.analysisToolStripMenuItem.Size = new System.Drawing.Size(88, 29);
            this.analysisToolStripMenuItem.Text = "Analysis";
            // 
            // fourierAttributesToolStripMenuItem
            // 
            this.fourierAttributesToolStripMenuItem.Name = "fourierAttributesToolStripMenuItem";
            this.fourierAttributesToolStripMenuItem.Size = new System.Drawing.Size(253, 30);
            this.fourierAttributesToolStripMenuItem.Text = "Fourier Attributes";
            this.fourierAttributesToolStripMenuItem.Click += new System.EventHandler(this.fourierAttributesToolStripMenuItem_Click);
            // 
            // numericUpDownGain
            // 
            this.numericUpDownGain.Enabled = false;
            this.numericUpDownGain.Location = new System.Drawing.Point(18, 135);
            this.numericUpDownGain.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.numericUpDownGain.Name = "numericUpDownGain";
            this.numericUpDownGain.Size = new System.Drawing.Size(74, 26);
            this.numericUpDownGain.TabIndex = 3;
            this.numericUpDownGain.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 75);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 20);
            this.label1.TabIndex = 4;
            this.label1.Text = "Gain:";
            // 
            // checkBoxAutoGain
            // 
            this.checkBoxAutoGain.AutoSize = true;
            this.checkBoxAutoGain.Checked = true;
            this.checkBoxAutoGain.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAutoGain.Location = new System.Drawing.Point(18, 100);
            this.checkBoxAutoGain.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkBoxAutoGain.Name = "checkBoxAutoGain";
            this.checkBoxAutoGain.Size = new System.Drawing.Size(69, 24);
            this.checkBoxAutoGain.TabIndex = 5;
            this.checkBoxAutoGain.Text = "Auto";
            this.checkBoxAutoGain.UseVisualStyleBackColor = true;
            this.checkBoxAutoGain.CheckedChanged += new System.EventHandler(this.checkBoxAutoGain_CheckedChanged);
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox1.Location = new System.Drawing.Point(100, 606);
            this.textBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(148, 26);
            this.textBox1.TabIndex = 6;
            // 
            // textBox2
            // 
            this.textBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox2.Location = new System.Drawing.Point(987, 605);
            this.textBox2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(148, 26);
            this.textBox2.TabIndex = 7;
            // 
            // labelStatus
            // 
            this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(256, 611);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(61, 20);
            this.labelStatus.TabIndex = 8;
            this.labelStatus.Text = "[status]";
            // 
            // FormTrace
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1155, 689);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.checkBoxAutoGain);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numericUpDownGain);
            this.Controls.Add(this.hScrollBarWindow);
            this.Controls.Add(this.pictureBoxEeg);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "FormTrace";
            this.Text = "Analyzer";
            this.Load += new System.EventHandler(this.FormTrace_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxEeg)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownGain)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxEeg;
        private System.Windows.Forms.HScrollBar hScrollBarWindow;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openFileToolStripMenuItem;
        private System.Windows.Forms.NumericUpDown numericUpDownGain;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBoxAutoGain;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.ToolStripMenuItem selectionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem selectionLabelsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem analysisToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fourierAttributesToolStripMenuItem;
    }
}

