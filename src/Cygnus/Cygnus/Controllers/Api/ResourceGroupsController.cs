using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Cygnus.Models;
using Cygnus.Models.Api;

namespace Cygnus.Controllers.Api
{
    public class ResourceGroupsController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ResourceGroupsController()
        {
            db.Configuration.LazyLoadingEnabled = true;
            db.Configuration.ProxyCreationEnabled = true;
        }

        // GET: api/ResourceGroups
        public ICollection<ResourceGroup> GetResourceGroups()
        {
            var groups = db.ResourceGroups.Include(g => g.Resources).ToArray();
            return groups;
        }

        // GET: api/ResourceGroups/5
        [ResponseType(typeof(ResourceGroup))]
        public async Task<IHttpActionResult> GetResourceGroup(Guid id)
        {
            ResourceGroup resourceGroup = await db.ResourceGroups.FindAsync(id);
            if (resourceGroup == null)
            {
                return NotFound();
            }

            return Ok(resourceGroup);
        }

        // PUT: api/ResourceGroups/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutResourceGroup(Guid id, ResourceGroup resourceGroup)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != resourceGroup.Id)
            {
                return BadRequest();
            }

            db.Entry(resourceGroup).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ResourceGroupExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/ResourceGroups
        [ResponseType(typeof(ResourceGroup))]
        public async Task<IHttpActionResult> PostResourceGroup(ResourceGroup resourceGroup)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.ResourceGroups.Add(resourceGroup);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ResourceGroupExists(resourceGroup.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = resourceGroup.Id }, resourceGroup);
        }

        // DELETE: api/ResourceGroups/5
        [ResponseType(typeof(ResourceGroup))]
        public async Task<IHttpActionResult> DeleteResourceGroup(Guid id)
        {
            ResourceGroup resourceGroup = await db.ResourceGroups.FindAsync(id);
            if (resourceGroup == null)
            {
                return NotFound();
            }

            db.ResourceGroups.Remove(resourceGroup);
            await db.SaveChangesAsync();

            return Ok(resourceGroup);
        }

        public async void AddResourcesToGroup(ResourceGroup group, IEnumerable<Resource> resources)
        {
            if (group == null) { return; }

            foreach (var resource in resources)
            {
                resource.ResourceGroupId = group.Id;
            }

            await db.SaveChangesAsync();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ResourceGroupExists(Guid id)
        {
            return db.ResourceGroups.Count(e => e.Id == id) > 0;
        }
    }
}