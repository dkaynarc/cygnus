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

        private ResourceMessage CreateResponse(IResource r)
        {
            var response = new ResourceMessage()
            {
                Command = "response",
                // Resources that are write-only (e.g. most actuators) will return nothing here
                Data = r.GetResourceData(),
                DataType = r.GetResourceDataType(),
                DataUnits = r.GetResourceDataUnits(),
                SenderGuid = r.Guid.ToString()
            };
            return response;
        }

        private ResourceMessage CreateDefaultResponse(string message)
        {
            var response = new ResourceMessage()
            {
                Command = "default",
                Data = message,
                SenderGuid = m_serverGuid.ToString()
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
                return CreateResponse(resource);
            }
            else
            { 
                return CreateDefaultResponse(String.Format("No resource with guid: '{0}' exists.", resource.ToString()));
            }
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
