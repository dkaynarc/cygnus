using Cygnus.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Cygnus.Controllers.Api
{
    public class SensorsController : ApiController
    {
        Sensor[] sensors = new Sensor[] 
        { 
            new Sensor { Id = 1, Name = "Temperature" }, 
            new Sensor { Id = 2, Name = "Camera" }, 
            new Sensor { Id = 3, Name = "" } 
        };

        [HttpGet]
        public IEnumerable<Sensor> All()
        {
            return sensors;
        }

        [HttpGet]
        public IHttpActionResult Sensor(int id)
        {
            var sensor = sensors.FirstOrDefault((p) => p.Id == id);
            if (sensor == null)
            {
                return NotFound();
            }
            return Ok(sensor);
        }

        [HttpGet]
        public IHttpActionResult Capabilities()
        {
            return Ok("Capabilities");
        }
    }
}
