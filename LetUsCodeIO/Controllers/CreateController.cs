using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Mvc;
using LetUsCodeIO.Models;
using System.Xml.Linq;

namespace LetUsCodeIO.Controllers
{
    public class CreateController : Controller
    {
        private const string sDefaultTask = "flashcards";
        private const string cCreateItems = "create-items.xml";
        
        // GET: Create
        public ActionResult Index()
        {
            List<CreateModels> m = getDescriptions();
            return View(m);
        }


        //
        // Helper function to get the descriptions
        //
        private List<CreateModels> getDescriptions()
        {
            List<CreateModels> Models = ControllerContext.HttpContext.Cache["CreateDescriptions"]
                as List<CreateModels>;
            if (Models == null)
            {
                //<TASK Name="flashcards" Icon="animal-flashcards-penguin.png" 
                //LinkUrl = "" LinkValue = "" >
                var request = HttpContext.Request;
                var appUrl = HttpRuntime.AppDomainAppVirtualPath;

                if (appUrl != "/") appUrl += "/";

                var baseUrl = string.Format("{0}://{1}{2}", request.Url.Scheme, request.Url.Authority, appUrl);


                string XmlPath = ControllerContext.HttpContext.
            Server.MapPath("~/App_Data/create-items.xml");
                XDocument Doc = XDocument.Load(XmlPath);
                IEnumerable<CreateModels> Values = from d in Doc.Descendants("TASK")
                             select new CreateModels()
                             { Name = d.Attribute("Name").Value, Icon = 
                                string.Format("{0}{1}", baseUrl, d.Attribute("Icon").Value)
                                    , LinkURL = d.Attribute("LinkUrl").Value,
                                        LinkValue =d.Attribute("LinkValue").Value
                                    , Description = d.Value, Title=d.Attribute("Title").Value };

                Models = new List<CreateModels>(Values);
                

                ControllerContext.HttpContext.Cache.Add("CreateDescriptions", Models,
                    new System.Web.Caching.CacheDependency(XmlPath),
                    DateTime.MaxValue, TimeSpan.FromDays(1), System.Web.Caching.CacheItemPriority.Normal, null);
               
            }
            return Models;
        }
        
        /// <summary>
        /// Get the infomration about a specific task
        /// </summary>
        /// <param name="Task"></param>
        /// <returns></returns>
        public ActionResult LessonPlans(string Task)
        {
            return View();
        }
        
        

        // POST: Create/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Create/Edit/5
        public ActionResult Edit(string id)
        {
            return View();
        }

        // POST: Create/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }


    }
}
