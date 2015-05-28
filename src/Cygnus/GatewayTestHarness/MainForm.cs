using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;


namespace Cygnus.GatewayTestHarness
{
    public partial class MainForm : Form
    {
        private GatewayHost m_gatewayHost = null;
        private List<IResourceControl> m_resourceControls = new List<IResourceControl>();
        public MainForm()
        {
            InitializeComponent();
            m_gatewayHost = new GatewayHost();
        }

        protected override void OnLoad(EventArgs e)
        {
            m_gatewayHost.Initialize();
            InitializeResourceControls();
            base.OnLoad(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            m_gatewayHost.Shutdown();
            base.OnFormClosing(e);
        }

        protected override void OnResize(EventArgs e)
        {
            m_resControlPanel.MaximumSize = this.Size;
            base.OnResize(e);
        }

        private void InitializeResourceControls()
        {
            foreach (var resource in ResourceManager.Instance.Resources)
            {
                var type = Type.GetType(resource.GetResourceDataType());
                if (type == typeof(System.Double))
                {
                    var slider = new ResourceSliderControl(resource);
                    m_resControlPanel.Controls.Add(slider);
                    m_resourceControls.Add(slider);
                }
                if (type == typeof(System.Boolean))
                {
                    var switchControl = new ResourceSwitchControl(resource);
                    m_resControlPanel.Controls.Add(switchControl);
                    m_resourceControls.Add(switchControl);
                }
            }
        }
    }
}
