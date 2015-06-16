using Cygnus.Managers;
using Cygnus.Nlp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Cygnus.Controllers.Api
{
    public class TestController : ApiController
    {
        public IHttpActionResult GetTest()
        {
            return Ok();
        }
    }
}
