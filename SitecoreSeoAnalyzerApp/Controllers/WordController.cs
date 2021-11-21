using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SitecoreSeoAnalyzerApp.Models;

namespace SitecoreSeoAnalyzerApp.Controllers
{
    public class WordController : Controller
    {
        private readonly ILogger<WordController> _logger;

        public WordController(ILogger<WordController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Todo : to be deleted
        /// </summary>
        public List<Word> PopulateDummyData()
        {
            var word1 = new Word("Automation", 1045);
            var word2 = new Word("Insanity", 9);
            var word3 = new Word("crazy", 2056);
            var word4 = new Word("random", 456);
            var word5 = new Word("Beauty", 21);
            var word6 = new Word("smart", 89);
            var word7 = new Word("rebound", 344);
            var word8 = new Word("Love", 788);
            var word9 = new Word("smooth", 23);
            var word10 = new Word("start", 467);
            var words = new List<Word>(){ word1, word2, word3, word4, word5, word6, word7, word8, word9, word10 };

            return words;
        }

        [HttpGet]
        public ActionResult Analyze(object sender, EventArgs e)
        {
            var result = Json(PopulateDummyData());
            return result;
        }
    }
}
