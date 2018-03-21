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
            this.numericUpDownGain = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.displayChannelsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkBoxAutoGain = new System.Windows.Forms.CheckBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxEeg)).BeginInit();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownGain)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBoxEeg
            // 
            this.pictureBoxEeg.Location = new System.Drawing.Point(67, 54);
            this.pictureBoxEeg.Name = "pictureBoxEeg";
            this.pictureBoxEeg.Size = new System.Drawing.Size(691, 333);
            this.pictureBoxEeg.TabIndex = 0;
            this.pictureBoxEeg.TabStop = false;
            this.pictureBoxEeg.Click += new System.EventHandler(this.pictureBoxEeg_Click);
            // 
            // hScrollBarWindow
            // 
            this.hScrollBarWindow.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.hScrollBarWindow.Location = new System.Drawing.Point(67, 419);
            this.hScrollBarWindow.Name = "hScrollBarWindow";
            this.hScrollBarWindow.Size = new System.Drawing.Size(694, 20);
            this.hScrollBarWindow.TabIndex = 1;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(770, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openFileToolStripMenuItem,
            this.displayChannelsToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openFileToolStripMenuItem
            // 
            this.openFileToolStripMenuItem.Name = "openFileToolStripMenuItem";
            this.openFileToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.openFileToolStripMenuItem.Text = "Open File";
            this.openFileToolStripMenuItem.Click += new System.EventHandler(this.openFileToolStripMenuItem_Click);
            // 
            // numericUpDownGain
            // 
            this.numericUpDownGain.Enabled = false;
            this.numericUpDownGain.Location = new System.Drawing.Point(12, 88);
            this.numericUpDownGain.Name = "numericUpDownGain";
            this.numericUpDownGain.Size = new System.Drawing.Size(49, 20);
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
            this.label1.Location = new System.Drawing.Point(13, 49);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Gain:";
            // 
            // displayChannelsToolStripMenuItem
            // 
            this.displayChannelsToolStripMenuItem.Name = "displayChannelsToolStripMenuItem";
            this.displayChannelsToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.displayChannelsToolStripMenuItem.Text = "Display Channels";
            this.displayChannelsToolStripMenuItem.Click += new System.EventHandler(this.displayChannelsToolStripMenuItem_Click);
            // 
            // checkBoxAutoGain
            // 
            this.checkBoxAutoGain.AutoSize = true;
            this.checkBoxAutoGain.Checked = true;
            this.checkBoxAutoGain.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAutoGain.Location = new System.Drawing.Point(12, 65);
            this.checkBoxAutoGain.Name = "checkBoxAutoGain";
            this.checkBoxAutoGain.Size = new System.Drawing.Size(48, 17);
            this.checkBoxAutoGain.TabIndex = 5;
            this.checkBoxAutoGain.Text = "Auto";
            this.checkBoxAutoGain.UseVisualStyleBackColor = true;
            this.checkBoxAutoGain.CheckedChanged += new System.EventHandler(this.checkBoxAutoGain_CheckedChanged);
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox1.Location = new System.Drawing.Point(67, 394);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 20);
            this.textBox1.TabIndex = 6;
            // 
            // textBox2
            // 
            this.textBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox2.Location = new System.Drawing.Point(658, 393);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(100, 20);
            this.textBox2.TabIndex = 7;
            // 
            // FormTrace
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(770, 448);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.checkBoxAutoGain);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numericUpDownGain);
            this.Controls.Add(this.hScrollBarWindow);
            this.Controls.Add(this.pictureBoxEeg);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
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
        private System.Windows.Forms.ToolStripMenuItem displayChannelsToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBoxAutoGain;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
    }
}

