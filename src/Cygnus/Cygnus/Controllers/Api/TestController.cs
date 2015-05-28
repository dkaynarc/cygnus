using Cygnus.Managers;
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
            NlpDecisionEngine.Instance.Test();
            return Ok();
        }
    }
}
