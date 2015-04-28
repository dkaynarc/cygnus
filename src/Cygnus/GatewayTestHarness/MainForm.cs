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
using WebSocketSharp;
using WebSocketSharp.Server;
using Cygnus.GatewayInterface;

namespace Cygnus.GatewayTestHarness
{
    public partial class MainForm : Form
    {
        private WebSocketServer m_gatewayServer;
        private Guid m_thisGatewayGuid;
        const string GuidFilePath = "guid.txt";
        const string ServerName = "CygnusGateway1";

        public MainForm()
        {
            InitializeComponent();

            InitializeGuid();

            m_gatewayServer = new WebSocketServer("ws://localhost:9300");
            BindResourceServices();
            OpenSocketServer();
            RegisterWithApi();
        }
     
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
        }

        private void InitializeGuid()
        {
            Guid g = Guid.NewGuid();
            if (File.Exists(GuidFilePath))
            {
                Guid.TryParse(File.ReadAllText(GuidFilePath), out g);
            }
            else
            {
                File.WriteAllText(GuidFilePath, g.ToString());
            }
            m_thisGatewayGuid = g;
        }

        private void RegisterWithApi()
        {
            var apiProxy = new CygnusApiProxy();
            apiProxy.PostGateway(new Gateway()
            {
                Id = m_thisGatewayGuid,
                Name = ServerName
            });
        }

        private void OpenSocketServer()
        {
            Debug.Assert(m_gatewayServer != null);
            m_gatewayServer.Start();
        }

        private void BindResourceServices()
        {
            Debug.Assert(m_gatewayServer != null);
            BindService("/temperature1", new MockTemperatureSensor());
        }

        private void BindService(string path, IResource r)
        {
            m_gatewayServer.AddWebSocketService<ResourceService>(path,
                () =>
                {
                    var rs = new ResourceService();
                    rs.BindToResource(r, m_thisGatewayGuid);
                    return rs;
                });
        }

        private void CloseSocketServer()
        {
            m_gatewayServer.Stop();
        }
    }
}
