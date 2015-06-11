using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cygnus.GatewayInterface;
using Cygnus.Models;

namespace Cygnus.Managers
{
    public class GatewayResourceProxy
    {
        private static GatewayResourceProxy m_instance;
        private ApplicationDbContext m_db = new ApplicationDbContext();
        public static GatewayResourceProxy Instance
        {
            get
            {
                if (m_instance == null) m_instance = new GatewayResourceProxy();
                return m_instance;
            }
        }
        
        private Dictionary<string, GatewaySocketClient> m_gatewayClients = new Dictionary<string, GatewaySocketClient>();

        private GatewayResourceProxy()
        {
        }
        
        public void RegisterAllResources()
        {
            var resources = m_db.Resources;
            foreach (var resource in resources)
            {
                if (!m_gatewayClients.ContainsKey(resource.Uri))
                {
                    var client = new GatewaySocketClient(resource.Uri);
                    m_gatewayClients.Add(resource.Uri, client);
                }
            }
        }
        
        public void RegisterResource(Models.Api.Resource resource)
        {
            if (!m_gatewayClients.ContainsKey(resource.Uri))
            {
                var client = new GatewaySocketClient(resource.Uri);
                m_gatewayClients.Add(resource.Uri, client);
            }
        }

        public void UnregisterResource(Models.Api.Resource resource)
        {
            if (m_gatewayClients.ContainsKey(resource.Uri))
            {
                m_gatewayClients.Remove(resource.Uri);
            }
        }

        public void UnregisterAllResources()
        {
            m_gatewayClients.Clear();
        }

        public Guid SendSetResourceDataRequest(Guid resourceId, object data, INotifiableRequester sender)
        {
            GatewaySocketClient client = null;
            Guid requestGuid = Guid.Empty;
            
            if (m_gatewayClients.TryGetValue(GetResourceUri(resourceId), out client))
            {
                requestGuid = client.SendSetResourceDataRequest(resourceId, data, sender);
            }

            return requestGuid;
        }

        public Guid SendGetResourceDataRequest(Guid resourceId, INotifiableRequester sender)
        {
            GatewaySocketClient client = null;
            Guid requestGuid = Guid.Empty;
            
            if (m_gatewayClients.TryGetValue(GetResourceUri(resourceId), out client))
            {
                requestGuid = client.SendGetResourceDataRequest(resourceId, sender);
            }
            
            return requestGuid;
        }

        public Guid SendSetCommunicationModeRequest(Guid resourceId, CommunicationMode mode)
        {
            GatewaySocketClient client = null;
            Guid requestGuid = Guid.Empty;

            if (m_gatewayClients.TryGetValue(GetResourceUri(resourceId), out client))
            {
                requestGuid = client.SendSetCommunicationMode(resourceId, mode);
            }

            return requestGuid;
        }

        public void RegisterOnMessageEvent(Guid resourceId, MessageReceivedHandler handler)
        {
            GatewaySocketClient client = null;

            if (m_gatewayClients.TryGetValue(GetResourceUri(resourceId), out client))
            {
                client.OnMessageReceived += handler;
            }
        }

        public void UnregisterOnMessageEvent(Guid resourceId, MessageReceivedHandler handler)
        {
            GatewaySocketClient client = null;

            if (m_gatewayClients.TryGetValue(GetResourceUri(resourceId), out client))
            {
                client.OnMessageReceived -= handler;
            }
        }

        private string GetResourceUri(Guid resourceId)
        {
            return m_db.Resources.Where(r => r.Id == resourceId).Select(r => r.Uri).First();
        }
    }
}
