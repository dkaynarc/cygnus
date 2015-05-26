using Cygnus.Managers;
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
        public JsonResult AutoComplete(string term)
        {
            var result = new List<KeyValuePair<string, string>>();

            IList<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem { Text = "test1", Value = "0" });
            list.Add(new SelectListItem { Text = "test2", Value = "1" });
            list.Add(new SelectListItem { Text = "test3", Value = "2" });
            list.Add(new SelectListItem { Text = "test4", Value = "3" });

            foreach (var item in list)
            {
                result.Add(new KeyValuePair<string, string>(item.Value.ToString(), item.Text));
            }

            var result3 = result.Where(s => s.Value.ToLower().Contains(term.ToLower())).Select(w => w).ToList();

            // Test Hook
            NlpDecisionEngine.Instance.Test();

            return Json(result3, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult Submit(string term)
        {
            NlpDecisionEngine.Instance.MakeQuery(term);
            // Temp
            return Json("OK");
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult GetDetail(int id)
        {
            TestModels.TestModel model = new TestModels.TestModel();

            if (id == 0)
            {
                model.Id = 1;
                model.Name = "Foo";
                model.Mobile = "525";
            }
            else
            {
                model.Id = 2;
                model.Name = "Bar";
                model.Mobile = "006";
            }
            return Json(model);
        }
    }
}

// Temporary
namespace Cygnus.TestModels
{
    public class TestModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Mobile { get; set; }
    }
}