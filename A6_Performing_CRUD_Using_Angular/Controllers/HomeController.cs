using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace A6_Performing_CRUD_Using_Angular.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var client = new HttpClient();
            var response = client.GetAsync("http://localhost:61984/api/EmployeesAPI/").Result;
            var employees = response.Content.ReadAsAsync<IEnumerable<EmployeesAPIController>>().Result;
            return View(employees);
           
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