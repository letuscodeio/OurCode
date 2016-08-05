using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LetUsCodeIO.Controllers
{
    public class HomeController : Controller
    {
      //  [OutputCache(Duration = 172800, VaryByParam = "none")]
        public ActionResult Index()
        {
            return View();
        }
        /*
        .. Removed to allow for more about information
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
        */
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

    }
}