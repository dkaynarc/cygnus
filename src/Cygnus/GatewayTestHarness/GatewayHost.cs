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
using System.Configuration;

namespace Cygnus.GatewayTestHarness
{
    public class GatewayHost
    {
        private WebSocketServer m_gatewayServer;
        private Guid m_thisGatewayGuid;
        const string GuidFilePath = "guid.txt";
        string GatewayName = "CygnusGateway1";
        string BaseUri = "ws://localhost:9300";

        public GatewayHost()
        {
            GatewayName = ConfigurationManager.AppSettings["GatewayName"];
            BaseUri = ConfigurationManager.AppSettings["GatewayBaseUri"];
            m_gatewayServer = new WebSocketServer(BaseUri);
        }

        public void Initialize()
        {
            InitializeGuid();
            CreateResources();
            CreateResourceService();
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
                Name = GatewayName
            });

            foreach (var resource in ResourceManager.Instance.Resources)
            {
                apiProxy.PostResource(new Resource()
                {
                    Id = resource.Guid,
                    Name = resource.Name,
                    Uri = BaseUri + "/resources",
                    Description = "",
                    GatewayId = m_thisGatewayGuid
                });
            }
        }

        private void OpenSocketServer()
        {
            Debug.Assert(m_gatewayServer != null);
            m_gatewayServer.Start();
        }

        private void CreateResources()
        {
            ResourceManager.Instance.Add(new MockTemperatureSensor("Temperature1"));
            ResourceManager.Instance.Add(new MockTemperatureSensor("Temperature2"));
            ResourceManager.Instance.Add(new MockSwitch("LightSwitch1"));
        }

        /// <summary>
        /// Creates a resource service endpoint for the resources held by ResourceManager.
        /// Note that the initializer doesn't execute until a request is made to the socket.
        /// </summary>
        private void CreateResourceService()
        {
            m_gatewayServer.AddWebSocketService<MultiplexedResourceService>("/resources",
                () =>
                {
                    var rs = new MultiplexedResourceService();
                    rs.Initialize(m_thisGatewayGuid);
                    ResourceManager.Instance.Resources.ForEach(x => rs.Bind(x));
                    return rs;
                });
        }

        private void CloseSocketServer()
        {
            m_gatewayServer.Stop();
        }
    }
}
