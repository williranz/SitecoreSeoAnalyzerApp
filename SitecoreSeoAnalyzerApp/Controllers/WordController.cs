using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

        [HttpPost]
        public ActionResult Analyze(string text, string url, bool opt1, bool opt2, bool opt3)
        {
            string cleanText = CleanTextInput(text);
            List<string> words = SplitStringIntoWords(cleanText);

            var options = new List<bool>() { opt1, opt2, opt3 };
            ProcessSeoAnalysis(words, url, options);
            
            var result = Json(PopulateDummyData());
            return result;
        }

        /// <summary>
        /// Todo : to be deleted
        /// </summary>
        private List<Word> PopulateDummyData()
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

        private string CleanTextInput(string input)
        {
            return null;
        }

        private List<string> SplitStringIntoWords(string cleanInput)
        {
            return null;
        }

        private void ProcessSeoAnalysis(List<string> words, string url, List<bool> options)
        {
            if (options.First())
            {
            }

            if (options.ElementAt(1))
            {
            }

            if (options.Last())
            {
            }
        }
    }
}
