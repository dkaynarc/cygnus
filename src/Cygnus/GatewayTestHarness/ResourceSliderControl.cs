using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cygnus.GatewayInterface;

namespace Cygnus.GatewayTestHarness
{
    public partial class ResourceSliderControl : UserControl, IResourceControl
    {
        private IResource m_boundResource = null;

        public ResourceSliderControl(IResource r = null)
        {
            InitializeComponent();
            ResetControls();
            if (r != null)
            {
                BindResource(r);
            }
        }

        public void BindResource(IResource resource)
        {
            m_boundResource = resource;
            m_groupBox.Text = resource.Name;
            AttachHandlers();

            int min = 0, max = 10;
            if (int.TryParse(m_boundResource.GetMin(), out min))
            {
                m_slider.Minimum = min;
            }
            if (int.TryParse(m_boundResource.GetMax(), out max))
            {
                m_slider.Maximum = max;
            }
        }

        public void UnbindResource()
        {
            RemoveHandlers();
            m_boundResource = null;
            ResetControls();
        }

        private void ResetControls()
        {
            m_groupBox.Text = "Unbound";
            m_slider.Value = 0;
            m_valueTextBox.Text = m_slider.Value.ToString();
            m_valueTextBox.BackColor = TextBox.DefaultBackColor;
        }

        private void AttachHandlers()
        {
            m_slider.ValueChanged += m_slider_ValueChanged;
            m_valueTextBox.TextChanged += m_valueTextBox_TextChanged;
            m_boundResource.OnDataChanged += OnDataChanged;
        }

        private void RemoveHandlers()
        {
            m_slider.ValueChanged -= m_slider_ValueChanged;
            m_valueTextBox.TextChanged -= m_valueTextBox_TextChanged;
            m_boundResource.OnDataChanged -= OnDataChanged;
        }

        void m_valueTextBox_TextChanged(object sender, EventArgs e)
        {
            int tempVal = 0;
            if (int.TryParse(m_valueTextBox.Text, out tempVal))
            {
                if (ValidateInput(tempVal))
                {
                    m_slider.Value = tempVal;
                    m_boundResource.SetResourceData(m_valueTextBox.Text);
                    m_valueTextBox.BackColor = TextBox.DefaultBackColor;
                }
                else
                {
                    m_valueTextBox.BackColor = Color.Red;
                }
            }
        }

        void m_slider_ValueChanged(object sender, EventArgs e)
        {
            m_valueTextBox.Text = m_slider.Value.ToString();
        }

        private bool ValidateInput(int value)
        {
            return (value >= m_slider.Minimum && value <= m_slider.Maximum);
        }

        private void OnDataChanged(object sender, DataChangedEventArgs e)
        {
            int tempVal = 0;
            if (int.TryParse(e.Data, out tempVal))
            {
                if (ValidateInput(tempVal))
                {
                    m_valueTextBox.Text = e.Data;
                    m_slider.Value = tempVal;
                }
            }
        }
    }
}
