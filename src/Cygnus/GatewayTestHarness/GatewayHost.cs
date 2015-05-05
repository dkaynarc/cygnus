using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WebSocketSharp;
using WebSocketSharp.Server;
using Cygnus.GatewayInterface;

namespace Cygnus.GatewayTestHarness
{
    public class GatewayHost
    {
        private WebSocketServer m_gatewayServer;
        private Guid m_thisGatewayGuid;
        const string GuidFilePath = "guid.txt";
        const string ServerName = "CygnusGateway1";
        private MultiplexedResourceService m_resourceService = null;

        public GatewayHost()
        {
            m_gatewayServer = new WebSocketServer("ws://localhost:9300");
        }

        public void Initialize()
        {
            InitializeGuid();
            BindResourceServices();
            OpenSocketServer();
            RegisterWithApi();
        }

        public void Shutdown()
        {
            CloseSocketServer();
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
            var resource = new MockTemperatureSensor("Temperature1");
            
            m_gatewayServer.AddWebSocketService<MultiplexedResourceService>("/resources",
                () =>
                {
                    var rs = new MultiplexedResourceService();
                    m_resourceService = rs;
                    return rs;
                });
        }

        private void BindService(string path, IResource r)
        {
            ResourceManager.Instance.Add(r);
            m_resourceService.Bind(r);
        }

        private void CloseSocketServer()
        {
            m_gatewayServer.Stop();
        }
    }
}
