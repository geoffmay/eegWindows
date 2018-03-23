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
        int m_picWidth = 691;
        int m_picHeight = 333;
        int m_picWidthMargin = -1;
        int m_picHeightMargin = -1;

        bool m_isPictureboxSelecting = false;
        int[] m_selections;

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

            pictureBoxEeg.Width = m_picWidth;
            pictureBoxEeg.Height = m_picHeight;
            m_picHeightMargin = this.Height - m_picHeight;
            m_picWidthMargin = this.Width - m_picWidth;
            setDefaults();
            loadSettings();
        }

        void pictureBoxEeg_MouseDown(object sender, MouseEventArgs e)
        {
            throw new Exception("The method or operation is not implemented.");
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
                    m_file.DrawSignal(bmp, true, m_startTime, m_startTime + m_windowDuration);
                    pictureBoxEeg.Image = bmp;
                    textBox1.Text = m_startTime.ToString();
                    textBox2.Text = (m_startTime + m_windowDuration).ToString();
                }
            }
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
                    loadData(m_lastFile);
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
        }
        
    }
}