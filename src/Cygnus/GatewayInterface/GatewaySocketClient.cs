using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;


namespace Cygnus.GatewayInterface
{
    public delegate void MessageReceivedHandler(object sender, MessageReceivedEventArgs e);
    public class GatewaySocketClient
    {
        public event MessageReceivedHandler OnMessageReceived;

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

        public Guid SendSetResourceDataRequest(Guid resourceId, object data, INotifiableRequester sender)
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
            return requestGuid;
        }

        public Guid SendGetResourceDataRequest(Guid resourceId, INotifiableRequester sender)
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
            return requestGuid;
        }

        public Guid SendSetCommunicationMode(Guid resourceId, CommunicationMode mode)
        {
            var requestGuid = Guid.NewGuid();
            var request = new ResourceMessage()
            {
                Command = "mode",
                Data = mode.ToString().ToLower(),
                TargetGuid = resourceId.ToString(),
                SenderGuid = m_clientId.ToString(),
                RequestGuid = requestGuid.ToString()
            };
            SendRequest(request);
            return requestGuid;
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
            if (response.RequestGuid != null)
            {
                var responseGuid = Guid.Parse(response.RequestGuid);
                if (m_requestList.TryGetValue(responseGuid, out originalRequest))
                {
                    originalRequest.Sender.Notify(responseGuid, response.Data);
                    m_requestList.Remove(responseGuid);
                }
            }
            else if (response.Command == "push-response")
            {
                // This might be a push style message with no corresponding request so fire the event
                this.RaiseOnMessageReceivedEvent(new MessageReceivedEventArgs()
                {
                    ResourceId = Guid.Parse(response.SenderGuid),
                    Data = response.Data,
                    DataUnits = response.DataUnits,
                    DataType = response.DataType
                });
            }
        }

        private void RaiseOnMessageReceivedEvent(MessageReceivedEventArgs e)
        {
            if (OnMessageReceived != null)
            {
                OnMessageReceived(this, e);
            }
        }
        
        private void SendRequest(ResourceMessage request)
        {
            
            if (m_gatewaySocket == null || m_gatewaySocket.ReadyState == WebSocketState.Closed)
            {
                Connect();
            }
            m_gatewaySocket.Send(JsonConvert.SerializeObject(request));
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

    internal class Request
    {
        public Guid Id;
        public INotifiableRequester Sender;
    }

    public class MessageReceivedEventArgs
    {
        public string Data { get; set; }
        public string DataType { get; set; }
        public string DataUnits { get; set; }
        public Guid ResourceId { get; set; }
    }
}
