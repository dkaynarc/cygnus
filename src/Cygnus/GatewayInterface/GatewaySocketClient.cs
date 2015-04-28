using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;


namespace Cygnus.GatewayInterface
{
    public class GatewaySocketClient
    {
        private WebSocket m_gatewaySocket = null;

        public GatewaySocketClient()
        {
        }

        private void Connect(string uri)
        {
            m_gatewaySocket = new WebSocket(uri);
            m_gatewaySocket.OnMessage += this.OnMessage;
            m_gatewaySocket.Connect();
        }

        private void Disconnect()
        {
            m_gatewaySocket.Close();
            m_gatewaySocket = null;
        }

        private void OnMessage(object sender, MessageEventArgs e)
        {
            var response = JsonConvert.DeserializeObject<ResourceMessage>(e.Data);
        }

        private void SendRequest(ResourceMessage request)
        {
            m_gatewaySocket.Send(JsonConvert.SerializeObject(request));
        }

#region Test Harness
        public static void Test()
        {
            var gwsc = new GatewaySocketClient();
            gwsc.Connect("ws://localhost:9300/temperature1");
            var request = new ResourceMessage()
            {
                Command = "get",
                SenderGuid = Guid.NewGuid().ToString()
            };
            gwsc.SendRequest(request);
        }
#endregion
    }
}
