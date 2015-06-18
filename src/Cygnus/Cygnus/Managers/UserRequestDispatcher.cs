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
        private TaskCompletionSource<UserResponsePackage> m_taskCompletion = null;
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
            using (var context = new ApplicationDbContext())
            {
                var resource = context.Resources.Where(r => r.Id == resourceId).First();
                resource.Description = description;
                context.Entry(resource).State = EntityState.Modified;

                try
                {
                    context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
            }
        }

        public UserResponsePackage GetResourceData(Guid resourceId)
        {
            Guid requestGuid = Guid.Empty;
            m_taskCompletion = new TaskCompletionSource<UserResponsePackage>();
            requestGuid = GatewayResourceProxy.Instance.SendGetResourceDataRequest(resourceId, this);
            var response = m_taskCompletion.Task.Result;
            m_taskCompletion = null;

            return response;
        }

        public void SetResourceData(Guid resourceId, object data)
        {
            var requestGuid = GatewayResourceProxy.Instance.SendSetResourceDataRequest(resourceId, data, this);
            m_taskCompletion = null;
        }

        public void Notify(Guid id, object data)
        {
            var userPackage = new UserResponsePackage() { Data = data };
            if (m_taskCompletion != null)
            {
                m_taskCompletion.SetResult(userPackage);
            }
        }
    }

    public class UserResponsePackage
    {
        public object Data { get; set; }
    }
}