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
    public class ResourceService : WebSocketBehavior
    {
        private IResource m_boundResource = null;
        private Guid m_resourceGuid;

        public bool IsBound
        {
            get
            {
                return (m_boundResource != null);
            }
        }
        public ResourceService()
        {
        }

        public void Bind(IResource r, Guid g)
        {
            if (r == null) throw new ArgumentNullException("Resource was null");

            m_boundResource = r;
            m_resourceGuid = g;
        }

        protected override void OnMessage(WebSocketSharp.MessageEventArgs e)
        {
            if (IsBound)
            {
                var request = JsonConvert.DeserializeObject<ResourceMessage>(e.Data);
                var response = PerformRequest(request);
                Send(JsonConvert.SerializeObject(response));
            }
        }

        private Cygnus.GatewayInterface.ResourceMessage CreateResponse()
        {
            var response = new Cygnus.GatewayInterface.ResourceMessage()
            {
                Command = "response",
                // Resources that are write-only (e.g. most actuators) will return nothing here
                Data = m_boundResource.GetResourceData(),
                DataType = m_boundResource.GetResourceDataType(),
                DataUnits = m_boundResource.GetResourceDataUnits(),
                SenderGuid = m_resourceGuid.ToString()
            };
            return response;
        }

        private Cygnus.GatewayInterface.ResourceMessage PerformRequest(ResourceMessage request)
        {
            if (request.Command.Contains("set"))
            {
                m_boundResource.SetResourceData(request.Data);
            }
            return CreateResponse();
        }
    }
}
