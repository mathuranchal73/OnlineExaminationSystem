using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OnlineExaminationSystem.Models;

namespace OnlineExaminationSystem.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var _ctx = new OESEntities();
            ViewBag.Tests = _ctx.Tests.Where(x => x.IsActive == 1).Select(x => new { x.Id, x.Name }).ToList();

            SessionModel _model = null;
            if (Session["SessionModel"] == null)
                _model = new SessionModel();
            else
                _model = (SessionModel)Session["SessionModel"];
            return View(_model);
        }

        public ActionResult Instructions(SessionModel model)
        {
            if(model!=null)
            {
                var _ctx = new OESEntities();
                var test = _ctx.Tests.Where(x => x.IsActive == 1 && x.Id == model.TestId).FirstOrDefault();
                if(test!=null)
                {
                    ViewBag.TestName = test.Name;
                    ViewBag.TestDescription = test.Description;
                    ViewBag.QuestionCount = test.TestXQuestions.Count;
                    ViewBag.TestDuration = test.DurationInMinute;
                }
            }
            return View();
        }

        public ActionResult Register()
        {
            return View();
        }
    }
}