using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cygnus.GatewayInterface;

namespace Cygnus.Managers
{
    public class GatewayManager
    {
        private static GatewayManager m_instance;
        public static GatewayManager Instance
        {
            get
            {
                if (m_instance == null) m_instance = new GatewayManager();
                return m_instance;
            }
        }
        
        private Dictionary<Guid, GatewaySocketClient> m_gatewayClients = new Dictionary<Guid, GatewaySocketClient>();

        private GatewayManager()
        {
        }
        
        private void RegisterAllGateways()
        {

        }
    }
}
