using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Cygnus.GatewayInterface
{
    public class MultiplexedResourceService : WebSocketBehavior
    {
        private HashSet<IResource> m_boundResources = null;
        private Guid m_serverGuid = Guid.NewGuid();

        public MultiplexedResourceService()
        {
            m_boundResources = new HashSet<IResource>();
        }
        public void Initialize(Guid serverGuid)
        {
            this.m_serverGuid = serverGuid;
        }

        public void Bind(IResource r)
        {
            if (r == null) throw new ArgumentNullException("Resource was null");

            m_boundResources.Add(r);
        }

        public void Unbind(IResource r)
        {
            m_boundResources.Remove(r);
        }

        protected override void OnMessage(WebSocketSharp.MessageEventArgs e)
        {
            var request = JsonConvert.DeserializeObject<ResourceMessage>(e.Data);
            var response = PerformRequest(request);
            Send(JsonConvert.SerializeObject(response));
        }

        private ResourceMessage CreateResponse(IResource r, string id)
        {
            var response = new ResourceMessage()
            {
                Command = "response",
                // Resources that are write-only (e.g. most actuators) will return nothing here
                Data = r.GetResourceData(),
                DataType = r.GetResourceDataType(),
                DataUnits = r.GetResourceDataUnits(),
                SenderGuid = r.Guid.ToString(),
                RequestGuid = id
            };
            return response;
        }

        private ResourceMessage CreateDefaultResponse(string message)
        {
            var response = new ResourceMessage()
            {
                Command = "default",
                Data = message,
                SenderGuid = m_serverGuid.ToString(),
                RequestGuid = Guid.NewGuid().ToString()
            };
            return response;
        }

        private ResourceMessage PerformRequest(ResourceMessage request)
        {
            var resourceGuid = new Guid(request.TargetGuid);
            var resource = GetResourceByGuid(resourceGuid);
            if (resource != null && resource.IsInitialized)
            {
                if (request.Command.Contains("set"))
                {
                    resource.SetResourceData(request.Data);
                }
                else if (request.Command.Contains("mode"))
                {
                    HandleModeChangeRequest(request);
                    return CreateDefaultResponse("OK");
                }
                return CreateResponse(resource, request.RequestGuid);
            }
            else
            { 
                return CreateDefaultResponse(String.Format("No resource with guid: '{0}' exists.", request.TargetGuid.ToString()));
            }
        }

        private void HandleModeChangeRequest(ResourceMessage message)
        {
            var newMode = message.Data;
            var resource = m_boundResources.Where(r => r.Guid == Guid.Parse(message.TargetGuid)).First();
            if (newMode == "push" && resource.CommunicationMode == CommunicationMode.Poll)
            {
                resource.OnDataChanged += OnResourceDataChanged;
                resource.CommunicationMode = CommunicationMode.Push;
            }
            else if (newMode == "poll" && resource.CommunicationMode == CommunicationMode.Push)
            {
                resource.OnDataChanged -= OnResourceDataChanged;
                resource.CommunicationMode = CommunicationMode.Poll;
            }
        }
        
        private void OnResourceDataChanged(object sender, DataChangedEventArgs e)
        {
            IResource s = (IResource)sender;
            if (s != null)
            {
                SendPushMessage(s, e.Data);
            }
        }

        private void SendPushMessage(IResource resource, string resData)
        {
            var message = new ResourceMessage()
            {
                Command = "push-response",
                Data = resData,
                DataType = resource.GetResourceDataType(),
                DataUnits = resource.GetResourceDataUnits(),
                SenderGuid = resource.Guid.ToString()
            };
            Send(JsonConvert.SerializeObject((message)));
        }

        #region Helpers
        private IResource GetResourceByGuid(Guid g)
        {
            return (from resource in m_boundResources
                    where resource.Guid == g
                    select resource).FirstOrDefault();
        }
        #endregion
    }
}
