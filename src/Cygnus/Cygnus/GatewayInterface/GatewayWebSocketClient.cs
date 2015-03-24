using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Web;
using System.Threading;
using System.Threading.Tasks;

namespace Cygnus.GatewayInterface
{
    public class GatewayWebSocketClient
    {
        private ClientWebSocket m_gatewayWebSocket = new ClientWebSocket();
        
        public GatewayWebSocketClient()
        {
        }

        public async Task Connect(string uri)
        {
            if (m_gatewayWebSocket.State != WebSocketState.Open && !String.IsNullOrEmpty(uri))
            {
                try
                {
                    var ct = new CancellationToken();
                    await m_gatewayWebSocket.ConnectAsync(new Uri(uri), ct);
                }
                catch (Exception e)
                {
                }
            }
        }

        public async Task Disconnect()
        {
            var ct = new CancellationToken();
            await m_gatewayWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                                                "Normal", ct);
        }

        public async Task<ISensorData> QuerySensorData(Guid sensorId)
        {
            var ct = new CancellationToken();
            await m_gatewayWebSocket.SendAsync(CreateSensorDataRequestMessage(sensorId), WebSocketMessageType.Text, true, ct);
            
            var responseBuffer = new ArraySegment<byte>();
            await m_gatewayWebSocket.ReceiveAsync(responseBuffer, ct);

            return ParseSensorData(responseBuffer);
        }

        private ISensorData ParseSensorData(ArraySegment<byte> socketData)
        {
            // STUB
            return new SensorDataBase();
        }

        private ArraySegment<byte> CreateSensorDataRequestMessage(Guid sensorId)
        {
            // STUB
            // Create JSON for this request
            return new ArraySegment<byte>();
        }
    }
}