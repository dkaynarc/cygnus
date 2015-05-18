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
        private Dictionary<Guid, Request> m_requestList = new Dictionary<Guid, Request>();
        private Guid m_clientId = Guid.NewGuid();
        private string m_uri;

        public GatewaySocketClient(string uri)
        {
            this.m_uri = uri;
        }
        
        ~GatewaySocketClient()
        {
            Disconnect();
        }

        public void SendSetResourceDataRequest(Guid resourceId, object data, INotifiableRequester sender)
        {
            Guid requestGuid = Guid.NewGuid();
            var request = new ResourceMessage()
            {
                Command = "set",
                SenderGuid = m_clientId.ToString(),
                TargetGuid = resourceId.ToString(),
                RequestGuid = requestGuid.ToString(),
                Data = data.ToString(),
                DataType = data.GetType().ToString()
            };
            m_requestList.Add(requestGuid, new Request { Id = requestGuid, Sender = sender });
            SendRequest(request);
        }

        public void SendGetResourceDataRequest(Guid resourceId, INotifiableRequester sender)
        {
            Guid requestGuid = Guid.NewGuid();
            var request = new ResourceMessage()
            {
                Command = "get",
                SenderGuid = m_clientId.ToString(),
                TargetGuid = resourceId.ToString(),
                RequestGuid = requestGuid.ToString(),
            };
            m_requestList.Add(requestGuid, new Request { Id = requestGuid, Sender = sender });
            SendRequest(request);
        }

        public void SendSetCommunicationMode(Guid resourceId, CommunicationMode mode)
        {
            var request = new ResourceMessage()
            {
                Command = "mode",
                Data = mode.ToString().ToLower(),
                TargetGuid = resourceId.ToString(),
                SenderGuid = m_clientId.ToString(),
                RequestGuid = Guid.NewGuid().ToString()
            };
            SendRequest(request);
        }

        private void Connect()
        {
            m_gatewaySocket = new WebSocket(m_uri);
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
            Request originalRequest = null;
            var responseGuid = Guid.Parse(response.RequestGuid);
            if (m_requestList.TryGetValue(responseGuid, out originalRequest))
            {
                originalRequest.Sender.Notify(response.Data);
                m_requestList.Remove(responseGuid);
            }
        }

        private void SendRequest(ResourceMessage request)
        {
            if (m_gatewaySocket.ReadyState == WebSocketState.Closed)
            {
                m_gatewaySocket.Send(JsonConvert.SerializeObject(request));
            }
        }

#region Test Harness
        public static void Test()
        {
            var gwsc = new GatewaySocketClient("ws://localhost:9300/resources");
            gwsc.Connect();
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

    public class Request
    {
        public Guid Id;
        public INotifiableRequester Sender;
    }
}
