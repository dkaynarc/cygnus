using Cygnus.Managers;
using Cygnus.Nlp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Cygnus.Controllers
{
    public class CoreRequestController : Controller
    {
        // GET: CoreRequest
        public ActionResult Index()
        {
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult Submit(string term)
        {
            IEnumerable<UserResponsePackage> responses = null;
            try
            {
                responses = NlpDecisionEngine.Instance.ExecuteQuery(term);
            }
            catch (Exception e)
            {
                return Json(e.Message);
            }

            object response = "OK";
            if (responses != null && responses.FirstOrDefault() != null)
            { 
                response = responses.FirstOrDefault().Data;
            }

            return Json(response);
        }
    }
}
