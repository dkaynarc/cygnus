﻿using System;
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
            SetButtonState(!m_buttonState);
            m_boundResource.SetResourceData(m_buttonState.ToString());
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
            m_buttonState = state;
            m_button.BackColor = state ? Color.Green : Color.Red;
            m_button.Text = state ? "ON" : "OFF";
        }
    }
}