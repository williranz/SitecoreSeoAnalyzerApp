using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

        private static readonly Regex _regexTextInput = new Regex("[^a-zA-Z0-9 ]");

        /// <summary>
        /// Word Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="hostingEnvironment"></param>
        public WordController(ILogger<WordController> logger, IHostingEnvironment hostingEnvironment)
        {
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
            _stopWords = LoadStopWords();
            
        }

        /// <summary>
        /// Analyze SEO of the page
        /// </summary>
        /// <param name="text"></param>
        /// <param name="url"></param>
        /// <param name="opt1"></param>
        /// <param name="opt2"></param>
        /// <param name="opt3"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Analyze(string text, string url, bool opt1, bool opt2, bool opt3)
        {
            string cleanText = CleanTextInput(text);
            
            List<string> grossWords = SplitStringIntoWords(cleanText);

            List<string> cleanWords = RemoveStopWords(grossWords);

            var options = new List<bool>() { opt1, opt2, opt3 };

            List<Word> finalResult = ProcessSeoAnalysis(cleanWords, url, options);
            
            var wordsResult = Json(finalResult);
            return wordsResult;
        }

        /// <summary>
        /// Remove symbols
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string CleanTextInput(string text)
        {
            string output = _regexTextInput.Replace(text, string.Empty);
            return output;
        }

        /// <summary>
        /// Get html body
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        string getHtmlBody(string html)
        {
            int startBodyTagIndex = html.IndexOf("<body", StringComparison.Ordinal);
            int endBodyTagIndex = html.IndexOf("</body>", StringComparison.Ordinal) + "</body>".Length;
            int bodyLength = endBodyTagIndex - startBodyTagIndex;
            string htmlBody = html.Substring(startBodyTagIndex, bodyLength);

            return htmlBody;
        }

        /// <summary>
        /// Get html head
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        string getHtmlHead(string html)
        {
            int startHeadTagIndex = html.IndexOf("<head", StringComparison.Ordinal);
            int endHeadTagIndex = html.IndexOf("</head>", StringComparison.Ordinal) + "</head>".Length;
            int headLength = endHeadTagIndex - startHeadTagIndex;
            string htmlHead = html.Substring(startHeadTagIndex, headLength);

            return htmlHead;
        }

        /// <summary>
        /// Get all meta tags inside html
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        string getHtmlMetaTags(string html)
        {
            string[] elementLines = html.Split(Environment.NewLine);
            string allMetas = string.Join(string.Empty, elementLines.Where(elementLine => elementLine.IndexOf("<meta", StringComparison.Ordinal) != -1).ToArray());
            return allMetas;
        }

        /// <summary>
        /// Get all external links inside html
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        string[] getExternalLinks(string html)
        {
            string[] links = { };
            return links;
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
        private List<Word> ProcessSeoAnalysis(List<string> words, string url, List<bool> options)
        {
            var webClient = new WebClient();
            byte[] rawByteContent;
            try
            {
                rawByteContent = webClient.DownloadData(url);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
           
            string rawTextContent = System.Text.Encoding.UTF8.GetString(rawByteContent);

            // Web body content
            string bodyHtml = getHtmlBody(rawTextContent);
            string bodyHtmlLine = bodyHtml.Replace(Environment.NewLine, " ");
            string[] WebTextContentSplit = bodyHtmlLine.Split(' ');

            // Meta tags content
            string headHtml = getHtmlHead(rawTextContent);
            string rawHtmlMetas = getHtmlMetaTags(headHtml);
            string[] wordInAllMetas = rawHtmlMetas.Split(' ');

            // External links content

            var resultWords = new List<Word>();
            foreach (var word in words)
            {
                var count = 0;
                var metaCount = 0;
                var extLinkCount = 0;

                // Option 1
                if (options.First())
                {
                    foreach (var wordSplit in WebTextContentSplit)
                    {
                        if (word.Equals(wordSplit, StringComparison.OrdinalIgnoreCase))
                        {
                            count++;
                        }
                    }
                }

                // Option 2
                if (options.ElementAt(1))
                {
                    foreach (var wordInAllMeta in wordInAllMetas)
                    {
                        if (word.Equals(wordInAllMeta, StringComparison.OrdinalIgnoreCase))
                        {
                            metaCount++;
                        }
                    }
                }

                // Option 3
                if (options.Last())
                {
                    foreach (var wordSplit in WebTextContentSplit)
                    {
                        if (word.Equals(wordSplit, StringComparison.OrdinalIgnoreCase))
                        {
                            extLinkCount++;
                        }
                    }
                }
                resultWords.Add(new Word(word, count, metaCount, extLinkCount));
            }
            return resultWords;
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
