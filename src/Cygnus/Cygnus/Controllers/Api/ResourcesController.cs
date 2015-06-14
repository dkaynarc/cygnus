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
using Cygnus.Managers;

namespace Cygnus.Controllers.Api
{
    public class ResourcesController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Resources
        public async Task<IQueryable<ResourceDTO>> GetResources()
        {
            var resources = await Task.Run(() =>
            {
                return from s in db.Resources
                              select new ResourceDTO()
                              {
                                  Id = s.Id,
                                  Name = s.Name,
                                  Uri = s.Uri,
                                  GatewayId = s.GatewayId
                              };
            });

            return resources;
        }

        // GET: api/Resources/5
        [ResponseType(typeof(Resource))]
        public async Task<IHttpActionResult> GetResource(Guid id)
        {
            var resource = await db.Resources.Include(s => s.Gateway).Select(s =>
                new ResourceDTO()
                {
                    Id = s.Id,
                    Name = s.Name,
                    Uri = s.Uri,
                    GatewayId = s.GatewayId
                }).SingleOrDefaultAsync(s => s.Id == id);
            if (resource == null)
            {
                return NotFound();
            }

            return Ok(resource);
        }

        // PUT: api/Resources/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutResource(Guid id, ResourceDTO resource)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != resource.Id)
            {
                return BadRequest();
            }

            var rawResource = ResourceFromResourceDTO(resource);

            db.Entry(rawResource).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ResourceExists(id))
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

        // POST: api/Resources
        [ResponseType(typeof(ResourceDTO))]
        public async Task<IHttpActionResult> PostResource(ResourceDTO resource)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var rawResource = ResourceFromResourceDTO(resource);

            if (ResourceExists(resource.Id))
            {
                db.Entry(rawResource).State = EntityState.Modified;
            }
            else
            {
                db.Resources.Add(rawResource);
                GatewayResourceProxy.Instance.RegisterResource(rawResource);
                await db.SaveChangesAsync();
            }
           
            return Ok(resource);
        }

        // DELETE: api/Resources/5
        [ResponseType(typeof(ResourceDTO))]
        public async Task<IHttpActionResult> DeleteResource(Guid id)
        {
            Resource resource = await db.Resources.FindAsync(id);
            if (resource == null)
            {
                return NotFound();
            }

            db.Resources.Remove(resource);
            await db.SaveChangesAsync();
            GatewayResourceProxy.Instance.UnregisterResource(resource);

            return Ok(ResourceDTOFromResource(resource));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ResourceExists(Guid id)
        {
            return db.Resources.Count(e => e.Id == id) > 0;
        }

        private Resource ResourceFromResourceDTO(ResourceDTO dto)
        {
            var resource = new Resource()
            {
                Id = dto.Id,
                Name = dto.Name,
                GatewayId = dto.GatewayId,
                Uri = dto.Uri,
            };
            return resource;
        }

        private ResourceDTO ResourceDTOFromResource(Resource resource)
        {
            var dto = new ResourceDTO()
            {
                Id = resource.Id,
                GatewayId = resource.GatewayId,
                Name = resource.Name,
                Uri = resource.Uri
            };
            return dto;
        }
    }
}