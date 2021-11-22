using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SitecoreSeoAnalyzerApp.Models;

namespace SitecoreSeoAnalyzerApp.Controllers
{
    public class WordController : Controller
    {
        private readonly ILogger<WordController> _logger;

        private readonly IHostingEnvironment _hostingEnvironment;

        private readonly List<string> _stopWords;

        private readonly Regex _regex;

        public WordController(ILogger<WordController> logger, IHostingEnvironment hostingEnvironment)
        {
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
            _regex = new Regex("[^a-zA-Z0-9 ]");
            _stopWords = LoadStopWords();
            
        }

        [HttpPost]
        public ActionResult Analyze(string text, string url, bool opt1, bool opt2, bool opt3)
        {
            string cleanText = CleanTextInput(text);
            
            List<string> grossWords = SplitStringIntoWords(cleanText);

            List<string> cleanWords = RemoveStopWords(grossWords);

            var options = new List<bool>() { opt1, opt2, opt3 };

            ProcessSeoAnalysis(cleanWords, url, options);
            
            var wordResult = Json(PopulateDummyData());
            return wordResult;
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

        /// <summary>
        /// Remove symbols
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string CleanTextInput(string text)
        {
            string output = _regex.Replace(text, string.Empty);
            return output;
        }

        /// <summary>
        /// split input text into words
        /// </summary>
        /// <param name="cleanText"></param>
        /// <returns></returns>
        private List<string> SplitStringIntoWords(string cleanText)
        {
            string[] words = cleanText.Split(' ');
            List<string> result = words.ToList();
            return result;
        }

        /// <summary>
        /// Remove stop words
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private List<string> RemoveStopWords(List<string> grossWords)
        {
            var result = grossWords;

            foreach (var word in _stopWords.Where(word => result.Contains(word, StringComparer.OrdinalIgnoreCase)))
            {
                result.Remove(word);
            }

            return result;
        }


        /// <summary>
        /// Process SEO analysis
        /// </summary>
        /// <param name="words"></param>
        /// <param name="url"></param>
        /// <param name="options"></param>
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

        /// <summary>
        /// Load stop words from csv
        /// </summary>
        /// <returns></returns>
        private List<string> LoadStopWords()
        {
            try
            {
                var stopWordsPath = Path.Combine(_hostingEnvironment.ContentRootPath, @"wwwroot\StopWords.csv");
                using var reader = new StreamReader(stopWordsPath);
                var listStopWords = new List<string>();
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    listStopWords.Add(line);
                }

                return listStopWords;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
