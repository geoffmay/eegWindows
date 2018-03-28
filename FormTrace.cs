using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Analysis
{
    public partial class FormTrace : Form
    {
        string m_settingsFile = "settings.txt";
        EDFfile m_file;
        double m_startTime;
        double m_sampleRate;
        double m_windowDuration;
        double m_gain;
        int m_picWidthMargin = -1;
        int m_picHeightMargin = -1;

        int m_currentSelectionMode = 0;
        int m_selectionStartIndex = -1;
        int m_leftClickCounter = 1;
        int m_rightClickCounter = -1;
        Bitmap m_baseBitmap;
        Bitmap m_overlayBitmap;
        int[] m_selections;
        bool[] m_displayChannel;

        string m_lastFile = "";
        public FormTrace()
        {
            //form events
            FormClosing += new FormClosingEventHandler(FormTrace_FormClosing);
            Resize += new EventHandler(FormTrace_Resize);
            MouseWheel += new MouseEventHandler(FormTrace_MouseWheel);

            //component events
            InitializeComponent();
            hScrollBarWindow.ValueChanged += new EventHandler(hScrollBarWindow_ValueChanged);
            pictureBoxEeg.MouseDown += new MouseEventHandler(pictureBoxEeg_MouseDown);
            pictureBoxEeg.MouseUp += new MouseEventHandler(PictureBoxEeg_MouseUp);
            pictureBoxEeg.MouseMove += PictureBoxEeg_MouseMove;
            numericUpDownGain.ValueChanged += NumericUpDownGain_ValueChanged;

            //pictureBoxEeg.Width = m_picWidth;
            //pictureBoxEeg.Height = m_picHeight;
            m_picHeightMargin = 745 - 333;
            m_picWidthMargin = 1177 - 691;
            //m_picHeightMargin = 745 - m_picHeight;
            //m_picWidthMargin = 1177 - m_picWidth;
            setDefaults();
            loadSettings();
        }

        private void PictureBoxEeg_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_currentSelectionMode != 0)
            {
                double startPixel = Math.Min(e.X, m_selectionStartIndex);
                double endPixel = Math.Max(e.X, m_selectionStartIndex);
                double sampleRate = m_file.SamplesPerSecondMax;
                double pixelToFrame = (double)sampleRate * m_windowDuration / (double)pictureBoxEeg.Width;
                double startFrameOffset = startPixel * pixelToFrame;
                double endFrameOffset = endPixel * pixelToFrame;
                double windowStartFrame = m_startTime * sampleRate;
                int startTime = (int)(windowStartFrame + startFrameOffset);
                int endTime = (int)(windowStartFrame + endFrameOffset);
                if (startTime != endTime)
                { 
                for (int i = startTime; i < endTime; i++)
                {
                    m_selections[i] = m_currentSelectionMode;
                }
                }
                Bitmap bmp = m_baseBitmap;
                drawOverlay(bmp);
                //pictureBoxEeg.Image = bmp;
            }
        }

        private void PictureBoxEeg_MouseUp(object sender, MouseEventArgs e)
        {
            m_currentSelectionMode = 0;
        }
        void pictureBoxEeg_MouseDown(object sender, MouseEventArgs e)
        {
            m_selectionStartIndex = e.X;
            if(e.Button == MouseButtons.Left)
            {
                m_currentSelectionMode = m_leftClickCounter;
                m_leftClickCounter++;
            }
            else if (e.Button == MouseButtons.Right)
            {
                m_currentSelectionMode = m_rightClickCounter;
                m_rightClickCounter--;
            }

        }

        private void NumericUpDownGain_ValueChanged(object sender, EventArgs e)
        {
            drawEeg();
        }


        void hScrollBarWindow_ValueChanged(object sender, EventArgs e)
        {
            m_startTime = hScrollBarWindow.Value;
            drawEeg();
        }

        void FormTrace_MouseWheel(object sender, MouseEventArgs e)
        {
            int newValue = (int)(hScrollBarWindow.Value + hScrollBarWindow.SmallChange * (double)(e.Delta / 120));
            newValue = Math.Max(newValue, hScrollBarWindow.Minimum);
            newValue = Math.Min(newValue, hScrollBarWindow.Maximum);
            hScrollBarWindow.Value = newValue;
        }


        void FormTrace_Resize(object sender, EventArgs e)
        {
            pictureBoxEeg.Width = this.Width - m_picWidthMargin;
            pictureBoxEeg.Height = this.Height - m_picHeightMargin;
            drawEeg();
        }

        void FormTrace_FormClosing(object sender, FormClosingEventArgs e)
        {
            saveSettings();
        }
        public void setDefaults()
        {
            m_startTime = 0;
            m_windowDuration = 6;
            m_gain = 1;

        }
        public void drawEeg()
        {
            if (m_file != null)
            {
                if (pictureBoxEeg.Width > 0 && pictureBoxEeg.Height > 0)
                {
                    Bitmap bmp = new Bitmap(pictureBoxEeg.Width, pictureBoxEeg.Height);
                    //draw eeg
                    if (checkBoxAutoGain.Checked)
                    {
                        m_file.DrawEeg(bmp, true, m_startTime, m_startTime + m_windowDuration);
                    }
                    else
                    {
                        m_file.DrawEeg(bmp, true, m_startTime, m_startTime + m_windowDuration, (double)numericUpDownGain.Value);
                    }
                    m_baseBitmap = bmp;
                    m_overlayBitmap = new Bitmap(m_baseBitmap.Width, m_baseBitmap.Height);
                    drawOverlay(bmp);

                    //update display
                    //pictureBoxEeg.Image = bmp;
                    textBox1.Text = m_startTime.ToString();
                    textBox2.Text = (m_startTime + m_windowDuration).ToString();


                }
            }
        }
        void drawOverlay(Bitmap bmp)
        {
            Graphics g = Graphics.FromImage(m_overlayBitmap);
            //Graphics g = Graphics.FromImage(m_overlayBitmap);
            g.DrawImage(bmp, new Point(0, 0));
            Brush b = new SolidBrush(Color.FromArgb(128, 0, 10, 128));
            double samplesPerSecond = m_file.SamplesPerSecondMax;
            int startIndex = (int)(m_startTime * samplesPerSecond);
            int endIndex = (int)((m_startTime + m_windowDuration) * samplesPerSecond);
            int selectionStart = -1;
            int i = startIndex;
            double maxSampleRate = m_file.SamplesPerSecondMax;
            bool lastSelected = m_selections[i] > 0;
            if(lastSelected)
            {
                selectionStart = 0;
            }
            i++;
            while (i < endIndex)
            { 
                bool isSelected = m_selections[i] > 0;
                if(isSelected!=lastSelected)
                {
                    if(lastSelected)
                    {
                        int xStart = (int)(selectionStart * bmp.Width / m_windowDuration / maxSampleRate);
                        int xEnd = (int)((i- startIndex) * bmp.Width / m_windowDuration / maxSampleRate);
                        g.FillRectangle(b, new Rectangle(xStart, 0, xEnd-xStart, bmp.Height));
                        //for (int x = xStart; x < xEnd; x++)
                        //{
                        //    for (int y = 0; y < bmp.Height; y++)
                        //    {
                        //        bmp.SetPixel(x, y, Color.CornflowerBlue);
                        //    }
                        //}
                    }
                    else
                    {
                        selectionStart = i -startIndex;
                    }
                    lastSelected = isSelected;
                }
                i++;
            }
            pictureBoxEeg.Image = m_overlayBitmap;
        }
        void saveSettings()
        {
            using (StreamWriter sWriter = new StreamWriter(m_settingsFile))
            {
                sWriter.WriteLine(m_lastFile);
            }
        }
        void loadSettings()
        {
            if (File.Exists(m_settingsFile))
            {
                using (StreamReader sReader = new StreamReader(m_settingsFile))
                {
                    m_lastFile = sReader.ReadLine();
                    if (File.Exists(m_lastFile))
                    {
                        loadData(m_lastFile);
                    }
                }
            }
        }
        void loadData(string filename)
        {
            if (filename != null && filename.Length > 1)
            {
                if (filename.ToLower().EndsWith(".edf") || filename.ToLower().EndsWith(".bdf"))
                {
                    m_file = EDFfile.OpenFile(filename);
                    int selectorCount = (int)(m_file.m_header.Duration.TotalSeconds * m_file.SamplesPerSecondMax);
                    m_selections = new int[selectorCount];
                }
                else
                {
                    throw new ArgumentException("unsupported file type");
                }
            }
            hScrollBarWindow.Minimum = 0;
            hScrollBarWindow.Maximum = (int)(Math.Ceiling(m_file.m_header.Duration.TotalSeconds));
            drawEeg();
        }
        void convertFile(string input, string output)
        {
            EDFfile file = EDFfile.OpenFile(input, true);
            file.convertToRaw(output);
        }

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            if (fd.ShowDialog() == DialogResult.OK)
            {
                loadData(fd.FileName);
                m_lastFile = fd.FileName;
            }
        }

        private void pictureBoxEeg_Click(object sender, EventArgs e)
        {

        }

        private void displayChannelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormChannelSelector form = new FormChannelSelector(this);
            form.Show();
        }

        private void FormTrace_Load(object sender, EventArgs e)
        {

        }

        public string[] AllChannelLabels
        {
            get
            {
                string label;
                string transducerType;
                string dimenstionUnit;
                double physicalMinimum;
                double physicalMaximum;
                double digitalMinimum;
                double digitalMaximum;
                string preFilter;
                double samplesPerRecord;
                string reserved;

                List<string> channels = new List<string>();
                for (int i = 0; i < m_file.m_header.NumberOfSignals; i++)
                {
                    m_file.m_header.GetSignalParameters(i, out label, out transducerType, out dimenstionUnit, out physicalMinimum, out physicalMaximum, out digitalMinimum, out digitalMaximum, out preFilter, out samplesPerRecord, out reserved);
                    channels.Add(label);
                }
                return channels.ToArray();
            }
        }

        private void checkBoxAutoGain_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownGain.Enabled = !checkBoxAutoGain.Checked;
            drawEeg();
        }
        
    }
}