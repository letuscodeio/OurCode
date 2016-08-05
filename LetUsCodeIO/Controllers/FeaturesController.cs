using LetUsCodeIO.App_Code;
using LetUsCodeIO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LetUsCodeIO.Controllers
{
    public class FeaturesController : Controller
    {
        

        // GET: Features
        public ActionResult Index()
        {
            return View();
        }



        /// <summary>
        /// Get the flashcards in the
        /// </summary>
        /// <returns></returns>
        /// 
  
        [Route("FlashCards/{Value}")]
        public ActionResult FlashCards(string Value)
        {
            List<FlashCardModels> FCModels =
                ControllerContext.HttpContext.Cache["FeaturesFlashCardAnimals"]
                as List<FlashCardModels>;

            if (string.IsNullOrWhiteSpace(Value))
            {
                int nAt = Request.RawUrl.IndexOf("FlashCards", StringComparison.InvariantCultureIgnoreCase);
                if ((nAt + 11) < Request.RawUrl.Length)
                {
                    Value = Request.RawUrl.Substring(nAt + 11);
                }
            }

            if (FCModels == null)
                FCModels = getAllFlashCards();
            var M = FCModels[0];
            if (!string.IsNullOrWhiteSpace(Value))
            {
                foreach (var item in FCModels)
                {
                    if(string.Compare(Value, item.Name, true) == 0)
                    {
                        M = item;
                        break;
                    }
                }
            }
            // Add default first one!
            return View(M);
        }

        //
        // Helper function to get all the questions
        //
        private List<FlashCardModels> getAllFlashCards()
        {
            List<FlashCardModels> FCModels = new List<FlashCardModels>();
            // Grab list of csv.
            string FlashCardCSV = ControllerContext.HttpContext.
                 Server.MapPath("~/Content/flashcards/animal-flashcards.csv");

            string[] Lines = LocalFileAccess.ReadFileLines(FlashCardCSV);
            // Build list of flash cards
            int Walker = -1;
            foreach (string Row in Lines)
            {
                // skip the first line
                Walker++;
                if (Walker == 0)
                    continue;
                string[] items = StringParser.FromCSV(Row, false);
                FlashCardModels Animal = new FlashCardModels()
                {
                    Name = items[0],
                    DisplayName = items[0].Replace('-', ' '),
                    FileName = items[1],
                    Clue1 = items[2],
                    Clue2 = items[3],
                    Clue3 = items[4],
                    Clue4 = items[5]
                };
                // Clean up displyname
                string [] DisplayNames = Animal.DisplayName.Split(' ');
                for (int i = 0; i < DisplayNames.Length; i++)
                {
                    string Name = DisplayNames[i];
                    if(string.Compare(Name.Trim(), "Large", true) != 0)
                    { 
                    Name = Char.ToUpper(Name[0]) + Name.Substring(1);
                    DisplayNames[i] = Name;
                    }
                }
                Animal.DisplayName = string.Join(" ", DisplayNames);
                FCModels.Add(Animal);


            }

            for (int i = 0; i < FCModels.Count; i++)
            {
                FlashCardModels FC = FCModels[i];
                if (i == 0)
                {
                    FC.Previous = FCModels[FCModels.Count - 1].Name;
                    FC.Next = FCModels[i + 1].Name;
                }
                else if (i == (FCModels.Count - 1))
                {
                    FC.Next = FCModels[i].Name;
                    FC.Previous = FCModels[i - 1].Name;

                }
                else
                {
                    FC.Previous = FCModels[i - 1].Name;
                    FC.Next = FCModels[i + 1].Name;
                }
            }

            ControllerContext.HttpContext.Cache.Add("FeaturesFlashCardAnimals", FCModels,
                new System.Web.Caching.CacheDependency(FlashCardCSV),
                DateTime.MaxValue, TimeSpan.FromDays(1), System.Web.Caching.CacheItemPriority.Normal, null);
            return FCModels;
        }
    }
}