using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Analysis
{
    public partial class FormChannelSelector : Form
    {
        FormTrace m_parent;
        string[] m_channelLabels;
        List<CheckBox> m_checkboxes;

        public FormChannelSelector(FormTrace parent)
        {
            m_parent = parent;
            InitializeComponent();
        }

        private void FormChannelSelector_Load(object sender, EventArgs e)
        {
            m_channelLabels = m_parent.AllChannelLabels;
            int x = 5;
            int y = buttonAll.Bottom + 5;
            for (int i = 0; i < m_channelLabels.Length; i++)
            {
                CheckBox cb = new CheckBox();
                cb.Text = m_channelLabels[i];
                bool isChecked = true;
                if (m_channelLabels.Length == m_parent.ChannelDisplay.Length)
                {
                    isChecked = m_parent.ChannelDisplay[i];
                }
                cb.Checked = isChecked;
                cb.SetBounds(x, y, cb.Width, cb.Height);
                cb.CheckedChanged += Cb_CheckedChanged;
                this.Controls.Add(cb);
                y += cb.Height + 5;
            }
        }

        private void Cb_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            
            m_parent.setSelection(cb.Text, cb.Checked);
        }

        private void buttonAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < this.Controls.Count; i++)
            {
                if(Controls[i] is CheckBox)
                {
                    CheckBox cb = Controls[i] as CheckBox;
                    cb.Checked = true;
                }
            }
        }

        private void buttonNone_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < this.Controls.Count; i++)
            {
                if (Controls[i] is CheckBox)
                {
                    CheckBox cb = Controls[i] as CheckBox;
                    cb.Checked = false;
                }
            }
        }
    }
}