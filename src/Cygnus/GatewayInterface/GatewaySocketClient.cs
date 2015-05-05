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
            gwsc.Connect("ws://localhost:9300/resources");
            var request = new ResourceMessage()
            {
                Command = "get",
                SenderGuid = Guid.NewGuid().ToString(),
                TargetGuid = (new Guid("e16c519e-d5f1-494d-b1ba-ed546a6bf199")).ToString()
            };
            gwsc.SendRequest(request);
        }
#endregion
    }
}
