using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

namespace Cygnus.GatewayInterface
{
    public class GatewayWebSocketClient
    {
        private WebSocket m_gatewayWebSocket = null;
        
        public GatewayWebSocketClient()
        {
        }

        public void Connect(string uri)
        {
            if (!String.IsNullOrEmpty(uri))
            {
                try
                {
                    m_gatewayWebSocket = new WebSocket(uri);
                    m_gatewayWebSocket.OnMessage += OnMessage;
                    m_gatewayWebSocket.Connect();
                }
                catch (Exception e)
                {
                }
            }
        }
        public void Disconnect()
        {
            m_gatewayWebSocket.CloseAsync();
        }

        public void QuerySensorData(Guid sensorId)
        {
            m_gatewayWebSocket.Send(CreateSensorDataRequestMessage(sensorId));
        }

        void OnMessage(object sender, MessageEventArgs e)
        {
            
        }

        private ISensorData ParseSensorData(ArraySegment<byte> socketData)
        {
            // STUB
            return new SensorDataBase();
        }

        private string CreateSensorDataRequestMessage(Guid sensorId)
        {
            // STUB
            // Create JSON for this request
            return String.Empty;
        }
    }
}