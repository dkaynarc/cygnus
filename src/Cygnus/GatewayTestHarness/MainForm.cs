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
        public MainForm()
        {
            InitializeComponent();
            m_gatewayHost = new GatewayHost();
            m_gatewayHost.Initialize();
        }
     
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            m_gatewayHost.Shutdown();
            base.OnFormClosing(e);
        }
    }
}
