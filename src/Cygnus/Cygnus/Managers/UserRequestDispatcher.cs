using Cygnus.GatewayInterface;
using Cygnus.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Cygnus.Managers
{
    public class UserRequestDispatcher : INotifiableRequester
    {
        private static UserRequestDispatcher m_instance;
        private ApplicationDbContext m_db = new ApplicationDbContext();
        private ConcurrentQueue<Guid> m_requestQueue = new ConcurrentQueue<Guid>();
        private ConcurrentDictionary<Guid, UserResponsePackage> m_responseBucket = new ConcurrentDictionary<Guid, UserResponsePackage>();
        private static AutoResetEvent m_waitEvent = new AutoResetEvent(false);
        public static UserRequestDispatcher Instance 
        {
            get
            {
                if (m_instance == null) m_instance = new UserRequestDispatcher();
                return m_instance;
            }
        }

        private UserRequestDispatcher()
        {
        }

        public void SetResourceDescription(Guid resourceId, string description)
        {
            var resource = m_db.Resources.Where(r => r.Id == resourceId).First();
            resource.Description = description;
            m_db.Entry(resource).State = EntityState.Modified;

            try
            {
                m_db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
        }

        public UserResponsePackage GetResourceData(Guid resourceId)
        {
            UserResponsePackage response = null;
            Task.Run(() =>
                {
                    var requestGuid = GatewayResourceProxy.Instance.SendGetResourceDataRequest(resourceId, this);
                    m_requestQueue.Enqueue(requestGuid);
                    m_waitEvent.WaitOne();

                    m_responseBucket.TryRemove(requestGuid, out response);
                });
            return response;
        }

        public void SetResourceData(Guid resourceId, object data)
        {
             var requestGuid = GatewayResourceProxy.Instance.SendSetResourceDataRequest(resourceId, data, this);
        }

        public void Notify(Guid id, object data)
        {
            var userPackage = new UserResponsePackage() { Data = data };
            m_responseBucket.AddOrUpdate(id, userPackage, (key, prev) => userPackage);
            m_waitEvent.Set();
        }
    }

    public class UserResponsePackage
    {
        public object Data { get; set; }
    }
}