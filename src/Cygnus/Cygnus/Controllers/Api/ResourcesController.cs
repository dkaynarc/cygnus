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
    public class ResourcesController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Sensors
        public async Task<IQueryable<ResourceDTO>> GetSensors()
        {
            var sensors = await Task.Run(() =>
            {
                return from s in db.Resources
                              select new ResourceDTO()
                              {
                                  Id = s.Id,
                                  Name = s.Name,
                                  Uri = s.Uri,
                                  Description = s.Description,
                                  GatewayName = s.Gateway.Name
                              };
            });

            //TEST
            Cygnus.GatewayInterface.GatewaySocketClient.Test();

            return sensors;
        }

        // GET: api/Sensors/5
        [ResponseType(typeof(Resource))]
        public async Task<IHttpActionResult> GetSensor(Guid id)
        {
            var sensor = await db.Resources.Include(s => s.Gateway).Select(s =>
                new ResourceDTO()
                {
                    Id = s.Id,
                    Name = s.Name,
                    Uri = s.Uri,
                    Description = s.Description,
                    GatewayName = s.Gateway.Name
                }).SingleOrDefaultAsync(s => s.Id == id);
            if (sensor == null)
            {
                return NotFound();
            }

            return Ok(sensor);
        }

        // PUT: api/Sensors/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutSensor(Guid id, Resource sensor)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != sensor.Id)
            {
                return BadRequest();
            }

            db.Entry(sensor).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SensorExists(id))
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

        // POST: api/Sensors
        [ResponseType(typeof(Resource))]
        public async Task<IHttpActionResult> PostSensor(Resource sensor)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Resources.Add(sensor);
            
            await db.SaveChangesAsync();
            db.Entry(sensor).Reference(s => s.Gateway).Load();

            var dto = new ResourceDTO()
            {
                Id = sensor.Id,
                Name = sensor.Name,
                Uri = sensor.Uri,
                Description = sensor.Description,
                GatewayName = sensor.Gateway.Name
            };

            return CreatedAtRoute("DefaultApi", new { id = sensor.Id }, dto);
        }

        // DELETE: api/Sensors/5
        [ResponseType(typeof(Resource))]
        public async Task<IHttpActionResult> DeleteSensor(Guid id)
        {
            Resource sensor = await db.Resources.FindAsync(id);
            if (sensor == null)
            {
                return NotFound();
            }

            db.Resources.Remove(sensor);
            await db.SaveChangesAsync();

            return Ok(sensor);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool SensorExists(Guid id)
        {
            return db.Resources.Count(e => e.Id == id) > 0;
        }
    }
}