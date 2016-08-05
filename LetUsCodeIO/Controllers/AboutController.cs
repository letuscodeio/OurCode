using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LetUsCodeIO.Controllers
{
    public class AboutController : Controller
    {
        // GET: About
        public ActionResult Index()
        {
            ViewBag.Message = "About Our Code";

            return View();
        }

        /// <summary>
        /// Get information about our parteners
        /// </summary>
        /// <returns></returns>
        public ActionResult OurPartners()
        {

            return View();
        }


        public ActionResult Presentation()
        {
            return View();
        }
    }
}