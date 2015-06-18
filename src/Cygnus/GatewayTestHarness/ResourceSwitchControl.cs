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
    public partial class ResourceSwitchControl : UserControl, IResourceControl
    {
        private IResource m_boundResource = null;
        private bool m_buttonState = false;

        public ResourceSwitchControl(IResource r = null)
        {
            InitializeComponent();
            if (r != null)
            {
                BindResource(r);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            this.ResetControls();
            base.OnLoad(e);
        }

        public void BindResource(IResource resource)
        {
            m_boundResource = resource;
            m_groupBox.Text = resource.Name;
            AttachHandlers();
        }

        public void UnbindResource()
        {
            m_boundResource = null;
        }

        private void ResetControls()
        {
            m_groupBox.Name = "Unbound";
            SetButtonState(false);
        }

        private void AttachHandlers()
        {
            m_button.Click += m_button_Click;
            m_boundResource.OnDataChanged += OnDataChanged;
        }

        void m_button_Click(object sender, EventArgs e)
        {
            m_boundResource.OnDataChanged -= OnDataChanged;
            SetButtonState(!m_buttonState);
            m_boundResource.SetResourceData(m_buttonState.ToString());
            m_boundResource.OnDataChanged += OnDataChanged;
        }

        private void RemoveHandlers()
        {
            m_button.Click -= m_button_Click;
            m_boundResource.OnDataChanged -= OnDataChanged;
        }

        private void OnDataChanged(object sender, DataChangedEventArgs e)
        {
            bool state = false;
            if (bool.TryParse(e.Data, out state))
            {
                SetButtonState(state);
            }
        }

        private void SetButtonState(bool state)
        {
            if (IsHandleCreated)
            {
                m_buttonState = state;
                this.BeginInvoke(new Action(() =>
                {
                    m_button.BackColor = state ? Color.Green : Color.Red;
                    m_button.Text = state ? "ON" : "OFF";
                }));
            }
        }
    }
}
