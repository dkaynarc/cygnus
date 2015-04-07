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
    public class GatewaysController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Gateways
        public IQueryable<Gateway> GetGateways()
        {
            return db.Gateways;
        }

        // GET: api/Gateways/5
        [ResponseType(typeof(Gateway))]
        public async Task<IHttpActionResult> GetGateway(Guid id)
        {
            Gateway gateway = await db.Gateways.FindAsync(id);
            if (gateway == null)
            {
                return NotFound();
            }

            return Ok(gateway);
        }

        // PUT: api/Gateways/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutGateway(Guid id, Gateway gateway)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (gateway == null || id != gateway.Id)
            {
                return BadRequest();
            }

            db.Entry(gateway).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GatewayExists(id))
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

        // POST: api/Gateways
        [ResponseType(typeof(Gateway))]
        public async Task<IHttpActionResult> PostGateway(Gateway gateway)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Gateways.Add(gateway);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (GatewayExists(gateway.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = gateway.Id }, gateway);
        }

        // DELETE: api/Gateways/5
        [ResponseType(typeof(Gateway))]
        public async Task<IHttpActionResult> DeleteGateway(Guid id)
        {
            Gateway gateway = await db.Gateways.FindAsync(id);
            if (gateway == null)
            {
                return NotFound();
            }

            db.Gateways.Remove(gateway);
            await db.SaveChangesAsync();

            return Ok(gateway);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool GatewayExists(Guid id)
        {
            return db.Gateways.Count(e => e.Id == id) > 0;
        }
    }
}