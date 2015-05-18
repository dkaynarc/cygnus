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

        public void SendSetResourceDataRequest(Guid resourceId, object data, INotifiableRequester sender)
        {
            GatewaySocketClient client = null;
            
            if (m_gatewayClients.TryGetValue(GetResourceUri(resourceId), out client))
            {
                client.SendSetResourceDataRequest(resourceId, data, sender);
            }
        }

        public void SendGetResourceDataRequest(Guid resourceId, INotifiableRequester sender)
        {
            GatewaySocketClient client = null;
            
            if (m_gatewayClients.TryGetValue(GetResourceUri(resourceId), out client))
            {
                client.SendGetResourceDataRequest(resourceId, sender);
            }
        }

        public void SendSetCommunicationModeRequest(Guid resourceId, CommunicationMode mode)
        {
            GatewaySocketClient client = null;

            if (m_gatewayClients.TryGetValue(GetResourceUri(resourceId), out client))
            {
                client.SendSetCommunicationMode(resourceId, mode);
            }
        }

        private string GetResourceUri(Guid resourceId)
        {
            return m_db.Resources.Where(r => r.Id == resourceId).Select(r => r.Uri).First();
        }
    }
}
