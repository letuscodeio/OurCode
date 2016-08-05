using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LetUsCodeIO.Models
{
    /// 
    /// <summary>Varibles used for flashcards </summary>
    /// 
    public class FlashCardModels
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string FileName { get; set; }
        public string Next { get; set; }
        public string Previous { get; set; }
        public string Clue1 { get; set; }
        public string Clue2 { get; set; }
        public string Clue3 { get; set; }
        public string Clue4 { get; set; }
    }
}