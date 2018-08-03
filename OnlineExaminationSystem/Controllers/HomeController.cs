using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlineExaminationSystem.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var _ctx = new OESEntities();
            ViewBag.Tests = _ctx.Tests.Where(x => x.IsActive == 1).Select(x => new { x.Id, x.Name }).ToList();
                
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}