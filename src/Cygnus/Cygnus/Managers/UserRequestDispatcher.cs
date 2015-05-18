using Cygnus.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;

namespace Cygnus.Managers
{
    public class UserRequestDispatcher
    {
        private static UserRequestDispatcher m_instance;
        private ApplicationDbContext m_db = new ApplicationDbContext();
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
    }
}