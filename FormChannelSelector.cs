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
            int y = 5;
            for (int i = 0; i < m_channelLabels.Length; i++)
            {
                CheckBox cb = new CheckBox();
                cb.Text = m_channelLabels[i];
                cb.Checked = true;
                cb.SetBounds(x, y, cb.Width, cb.Height);
                this.Controls.Add(cb);
                y += cb.Height + 5;
            }
        }
    }
}