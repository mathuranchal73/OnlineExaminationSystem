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
            var _ctx = new OESEntities1();
            ViewBag.Tests = _ctx.Tests.Where(x => x.IsActive == 1).Select(x => new { x.Id, x.Name }).ToList();

            SessionModel _model = null;
            if (Session["SessionModel"] == null)
            {
                _model = new SessionModel();
            }
               
            else
            {
                _model = (SessionModel)Session["SessionModel"];
            }
               
            return View(_model);
        }

        public ActionResult Instructions(SessionModel model)
        {
            if(model!=null)
            {
                var _ctx = new OESEntities1();
                var test = _ctx.Tests.Where(x => (x.IsActive == 1) && x.Id == model.TestId).FirstOrDefault();
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

        public ActionResult Register(SessionModel model)
        {

            Session["SessionModel"] = model;

            if (model == null || string.IsNullOrEmpty(model.UserName) || model.TestId < 1)
            {
                TempData["message"] = "Invalid Registration details. Please try again";
                return RedirectToAction("Index");
            }

            var _ctx = new OESEntities1();
            //to register the user

            Student _user = _ctx.Students.Where(x => x.Name.Equals(model.UserName, StringComparison.InvariantCultureIgnoreCase)
             && ((string.IsNullOrEmpty(model.Email) && string.IsNullOrEmpty(x.Email)) || (x.Email == model.Email))
             && ((string.IsNullOrEmpty(model.Phone) && string.IsNullOrEmpty(x.Phone)) || (x.Phone == model.Phone))).FirstOrDefault();

            if (_user == null)
            { 
                _user = new Student()
                {
                    Name = model.UserName,
                    Email = model.Email,
                    Phone = model.Phone,
                    EntryDate = DateTime.UtcNow,
                    AccessLevel = "Normal"
                };

            _ctx.Students.Add(_user);
            _ctx.SaveChanges();

             }

            Registration registration = _ctx.Registrations.Where(x => x.StudentId == _user.Id
            && x.TestId == model.TestId
            && x.TokenExpireTime > DateTime.UtcNow).FirstOrDefault();

            if(registration!=null)
            {
                this.Session["TOKEN"] = registration.Token;
                this.Session["TOKENEXPIRE"] = registration.TokenExpireTime;

            }

            else
            {
                Test test = _ctx.Tests.Where(x => (x.IsActive == 1) && x.Id == model.TestId).FirstOrDefault();
                if(test!=null)
                {
                    Registration newRegistration = new Registration();
                    {
                        newRegistration.RegistrationDate = DateTime.UtcNow;
                        newRegistration.TestId = model.TestId;
                        newRegistration.Token = Guid.NewGuid();
                        newRegistration.TokenExpireTime = DateTime.UtcNow.AddMinutes((double)test.DurationInMinute);
                    }

                    _user.Registrations.Add(newRegistration);
                    _ctx.Registrations.Add(newRegistration);
                    _ctx.SaveChanges();
                    this.Session["TOKEN"] = newRegistration.Token;
                    this.Session["TOKENEXPIRE"] = newRegistration.TokenExpireTime;

                }
            }
            return RedirectToAction("EvalPage", new {@token=Session["TOKEN"]});
        }

        public ActionResult EvalPage(Guid token, int? qno)
        {
            if (token == null)
            {
                TempData["message"] = "Invalid Token Details.Please reregister and try again";
                return RedirectToAction("Ïndex");
            }
                
       
            var _ctx = new OESEntities1();
            var registration = _ctx.Registrations.Where(x => x.Token.Equals(token)).FirstOrDefault();
            if (registration == null)
            {
                TempData["message"] = "This token is invalid";
                return RedirectToAction("Ïndex");
            }

            if(registration.TokenExpireTime<DateTime.UtcNow)
            {
                TempData["message"] = "The exam duration has expired at"+registration.TokenExpireTime.ToString();
                return RedirectToAction("Ïndex");
            }

            if (qno.GetValueOrDefault() < 1)
                qno = 1;
            var testQuestionId = _ctx.TestXQuestions
                .Where(x => x.TestId == registration.TestId && x.QuestionNumber == qno)
                .Select(x => x.Id).FirstOrDefault();

            if(testQuestionId>0)
            {
                var _model = _ctx.TestXQuestions.Where(x => x.Id == testQuestionId)
                    .Select(x => new QuestionModel()
                    {
                        QuestionType = x.Question.QuestionType,
                        QuestionNumber = (int)x.QuestionNumber,
                        Question = x.Question.Question1,
                        Point = (int)x.Question.Points,
                        TestId = x.Test.Id,
                        TestName = x.Test.Name,
                        options = x.Question.Choices.Where(y => (y.isActive == 1)).Select(y => new QuestionModel.QXOModel()
                        {
                            ChoiceId = y.Id,
                            Label = y.Label,


                        }).ToList()
                    }).FirstOrDefault();

                _model.TotalQuestionInSet = _ctx.TestXQuestions.Where(x => (x.Question.isActive == 1) && x.TestId == registration.TestId).Count();

                return View(_model);
            }

            else
            {
                return View("Error");
            }


        }
    }
}
