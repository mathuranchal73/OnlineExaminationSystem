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
            var _ctx = new OESEntities2();
            ViewBag.Tests = _ctx.Tests.Where(x => (x.IsActive == 1)).Select(x => new { x.Id, x.Name }).ToList();

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
                var _ctx = new OESEntities2();
                var test = _ctx.Tests.Where(x => (x.IsActive == 1) && x.Id == model.TestId).FirstOrDefault();
                if(test!=null)
                {
                    ViewBag.TestName = test.Name;
                    ViewBag.TestDescription = test.Description;
                    ViewBag.QuestionCount = test.TestXQuestions.Count;
                    ViewBag.TestDuration = test.DurationInMinute;
                }
            }
            return View(model);
        }

        public ActionResult Register(SessionModel model)
        {

            Session["SessionModel"] = model;

            if (model == null || string.IsNullOrEmpty(model.UserName) || model.TestId < 1)
            {
                TempData["message"] = "Invalid Registration details. Please try again";
                return RedirectToAction("Index");
            }

            var _ctx = new OESEntities2();
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
                
       
            var _ctx = new OESEntities2();

            //verify that the user is registered and is allowed to check the question
            var registration = _ctx.Registrations.Where(x => x.Token==token).FirstOrDefault();
            if (registration.Token == null)
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
                        options = x.Question.Choices.Where(y => (y.isActive == 1)).Select(y => new QXOModel()
                        {
                            ChoiceId = y.Id,
                            Label = y.Label,


                        }).ToList()
                    }).FirstOrDefault();

                //now if the answer is already answered earlier, set the choice of the user

                var savedAnswers = _ctx.TestXPapers.Where(x => x.TestXQuestionId == testQuestionId && x.RegistrationId == registration.Id && (x.Choice.isActive == 1))
                    .Select(x => new { x.ChoiceId, x.Answer }).ToList();

                foreach(var savedAnswer in savedAnswers)
                {
                    _model.options.Where(x => x.ChoiceId == savedAnswer.ChoiceId).FirstOrDefault().Answer = savedAnswer.Answer;
                }


                _model.TotalQuestionInSet = _ctx.TestXQuestions.Where(x => (x.Question.isActive == 1) && x.TestId == registration.TestId).Count();
                ViewBag.TimeExpire = registration.TokenExpireTime;
                return View(_model);
            }

            else
            {
                return View("Error");
            }

        }

        public ActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }


        [HttpPost]
        public ActionResult PostAnswer(AnswerModel choices)
        {
            var _ctx = new OESEntities2();
            //verify that the user is a registered and is allowed to check the question
            var registration = _ctx.Registrations.Where(x => (x.Token==choices.Token)).FirstOrDefault();
            if (registration == null)
            {
                TempData["message"] = "This token is invalid";
                return RedirectToAction("Ïndex");
            }
            else if (registration.TokenExpireTime < DateTime.UtcNow)
            {
                TempData["message"] = "The exam duration has expired at" + registration.TokenExpireTime.ToString();
                return RedirectToAction("Ïndex");
            }

            var testQuestionInfo = _ctx.TestXQuestions.Where(x => x.TestId == registration.TestId
              && x.QuestionNumber == choices.QuestionId)
            .Select(x => new
            {
                TQId = x.Id,
                QT = x.Question.QuestionType,
                QID = x.QuestionId,
                POINT = (double)x.Question.Points
            }).FirstOrDefault();

            if(testQuestionInfo!=null)
            {
                if (choices.UserChoices.Count > 1)
                {
                    List<TestXPaper> allPointValueOfChoices =
                        (
                        from a in _ctx.Choices.Where(x => (x.isActive == 1))
                        join b in choices.UserSelectedId on a.Id equals b
                        select new { a.Id, Points = (double)a.Points }).AsEnumerable()
                        .Select(x => new TestXPaper()
                        {
                            RegistrationId = registration.Id,
                            TestXQuestionId = testQuestionInfo.TQId,
                            ChoiceId = x.Id,
                            Answer = "CHECKED",
                            MarkScored = Math.Floor((decimal)testQuestionInfo.POINT / 100.0M) * (decimal)(x.Points)
                        }
                        ).ToList();

                    _ctx.TestXPapers.AddRange(allPointValueOfChoices);
                    
                }
                else
                {
                    //the answer is of type TEXT
                    _ctx.TestXPapers.Add(new TestXPaper()
                    {
                        RegistrationId = registration.Id,
                        TestXQuestionId = testQuestionInfo.QID,
                        ChoiceId = choices.UserChoices.FirstOrDefault().ChoiceId,
                        MarkScored = 10,
                        Answer=choices.Answer
                    });

                
                }

                _ctx.SaveChanges();
            }

            //get the next question depending on the direction

            var nextQuestionNumber = 1;
            
            if (choices.Direction.Equals("forward", StringComparison.CurrentCultureIgnoreCase))
            {
                nextQuestionNumber =Convert.ToInt32( _ctx.TestXQuestions.Where(x => x.TestId == choices.TestId
                && x.QuestionNumber > choices.QuestionId)
               .OrderBy(x => x.QuestionNumber).Take(1).Select(x => x.QuestionNumber).FirstOrDefault());
                
                
            }

            else
            {
                nextQuestionNumber = Convert.ToInt32(_ctx.TestXQuestions.Where(x => x.TestId == choices.TestId
                && x.QuestionNumber > choices.QuestionId)
               .OrderByDescending(x => x.QuestionNumber).Take(1).Select(x => x.QuestionNumber).FirstOrDefault());
            }

            if (nextQuestionNumber < 1)

                nextQuestionNumber = 1;
           
            
           

                return RedirectToAction("EvalPage", new
                {
                    @token = Session["TOKEN"],
                    @qno = nextQuestionNumber
                });
        }


       

    }
}
