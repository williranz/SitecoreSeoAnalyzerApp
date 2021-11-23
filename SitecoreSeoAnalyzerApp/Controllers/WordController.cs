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
    /// <summary>
    /// Word controller class handling all word processing
    /// </summary>
    public class WordController : Controller
    {
        /// <summary>
        /// Default logger
        /// </summary>
        private readonly ILogger<WordController> _logger;

        /// <summary>
        /// Hosting enviroment properties
        /// </summary>
        private readonly IHostingEnvironment _hostingEnvironment;

        /// <summary>
        /// Stop words properties
        /// </summary>
        private readonly List<string> _stopWords;

        /// <summary>
        /// Regex for alpha numeric
        /// </summary>
        private static readonly Regex _regexTextInput = new Regex("[^a-zA-Z0-9 ]");

        /// <summary>
        /// Word Constructor
        /// </summary>
        /// <param name="logger">default logger</param>
        /// <param name="hostingEnvironment">hosting environment location</param>
        public WordController(ILogger<WordController> logger, IHostingEnvironment hostingEnvironment)
        {
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
            _stopWords = LoadStopWords();
            
        }

        /// <summary>
        /// Analyze SEO of the page
        /// </summary>
        /// <param name="text">Input text</param>
        /// <param name="url">A page url</param>
        /// <param name="opt1">Option 1</param>
        /// <param name="opt2">Option 2</param>
        /// <param name="opt3">Option 3</param>
        /// <returns>Json data for each word</returns>
        [HttpPost]
        public ActionResult Analyze(string text, string url, bool opt1, bool opt2, bool opt3)
        {
            string cleanText = CleanTextInput(text);
            
            List<string> grossWords = SplitStringIntoWords(cleanText);

            IEnumerable<string> cleanWords = RemoveStopWords(grossWords);

            List<string> cleanDistinctWords = cleanWords.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            var options = new List<bool>() { opt1, opt2, opt3 };

            List<Word> finalResult = ProcessSeoAnalysis(cleanDistinctWords, url, options);
            
            var wordsResult = Json(finalResult);
            return wordsResult;
        }

        /// <summary>
        /// Remove symbols
        /// </summary>
        /// <param name="text"></param>
        /// <returns>Clean text</returns>
        private static string CleanTextInput(string text)
        {
            string output = _regexTextInput.Replace(text, string.Empty);
            return output;
        }

        /// <summary>
        /// Get html body from full html document
        /// </summary>
        /// <param name="html"></param>
        /// <returns>Html body element</returns>
        static string getHtmlBody(string html)
        {
            int startBodyTagIndex = html.IndexOf("<body", StringComparison.Ordinal);
            int endBodyTagIndex = html.IndexOf("</body>", StringComparison.Ordinal) + "</body>".Length;
            int bodyLength = endBodyTagIndex - startBodyTagIndex;
            string htmlBody = html.Substring(startBodyTagIndex, bodyLength);

            return htmlBody;
        }

        /// <summary>
        /// Get html head from full html document
        /// </summary>
        /// <param name="html"></param>
        /// <returns>Html head element</returns>
        static string getHtmlHead(string html)
        {
            int startHeadTagIndex = html.IndexOf("<head", StringComparison.Ordinal);
            int endHeadTagIndex = html.IndexOf("</head>", StringComparison.Ordinal) + "</head>".Length;
            int headLength = endHeadTagIndex - startHeadTagIndex;
            string htmlHead = html.Substring(startHeadTagIndex, headLength);

            return htmlHead;
        }

        /// <summary>
        /// Get text inside html body
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        static string[] getTextInsideHtmlBody(string html)
        {
            string[] bodyElementLines = html.Split(Environment.NewLine);
            List<string> bodyElementList = new List<string>();
            
            foreach (var bodyElementLine in bodyElementLines)
            {
                string bodyElementLineTrimmed = bodyElementLine.Trim();
                
                if (!string.IsNullOrWhiteSpace(bodyElementLineTrimmed))
                {
                    bodyElementList.Add(bodyElementLineTrimmed);
                }
            }
            var allBodyContentText = string.Join(" ", bodyElementList.ToArray());
            var allBodyTextSplit = allBodyContentText.Split(' ');
            return allBodyTextSplit;
        }

        /// <summary>
        /// Get all meta tags inside html
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private static string[] GetTextInsideHtmlMetaTags(string html)
        {
            string[] elementLines = html.Split(Environment.NewLine);
            List<string> allMetaContent = new List<string>();
            foreach (var elementLine in elementLines)
            {
                if (elementLine.IndexOf("<meta", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    // Trim white spaces
                    string elementLineTrimmed = elementLine.Trim();

                    // get all meta content in 1 line
                    string[] metaLines = elementLineTrimmed.Split("<meta");

                    foreach (var metaLine in metaLines)
                    {
                        if (metaLine.IndexOf("content=\"", StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            int startMetaTagIndex = metaLine.IndexOf("content=\"", StringComparison.OrdinalIgnoreCase) + "content=\"".Length;
                            string metaContent = metaLine.Substring(startMetaTagIndex);
                            int endMetaTagIndex = metaContent.IndexOf("\"", StringComparison.CurrentCulture) - "\"".Length;
                            metaContent = metaContent.Substring(0, endMetaTagIndex);
                            allMetaContent.Add(metaContent);
                        }
                    }
                }
            }
            var allMetaContentText = string.Join(" ", allMetaContent.ToArray());
            var allMetaContentTextSplit = allMetaContentText.Split(' '); 
            return allMetaContentTextSplit;
        }

        /// <summary>
        /// Get all external links inside html
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private static string[] GetTextInsideExternalLinks(string html)
        {
            string[] elementLines = html.Split(Environment.NewLine);
            List<string> allLinksLinesRaw = new List<string>();
            List<string> allLinksLines = new List<string>();

            foreach (var elementLine in elementLines)
            {
                if (elementLine.IndexOf("<a href=\"http", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    string elementLineTrimmed = elementLine.Trim();
                    allLinksLinesRaw.Add(elementLineTrimmed);
                }
            }

            foreach (var linksLine in allLinksLinesRaw)
            {
                int startLinkTextIndex = linksLine.IndexOf("<a href=\"http", StringComparison.OrdinalIgnoreCase);
                int endLinkTextIndex = linksLine.IndexOf(">", StringComparison.CurrentCulture) + ">".Length;
                string linkTextRemoved = linksLine.Remove(startLinkTextIndex, endLinkTextIndex);
                allLinksLines.Add(linkTextRemoved);
            }
            var allMetaContentText = allLinksLines.ToArray();
            return allMetaContentText;
        }

        /// <summary>
        /// split input text into words
        /// </summary>
        /// <param name="cleanText"></param>
        /// <returns></returns>
        private static List<string> SplitStringIntoWords(string cleanText)
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
        private IEnumerable<string> RemoveStopWords(List<string> grossWords)
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
                _logger.LogError(e.ToString());
                throw;
            }
            string rawTextContent = System.Text.Encoding.UTF8.GetString(rawByteContent);

            // Web body text
            string bodyHtml = getHtmlBody(rawTextContent);
            string[] wordsInBody = getTextInsideHtmlBody(bodyHtml);

            // Meta tags content
            string headHtml = getHtmlHead(rawTextContent);
            string[] wordsInAllMetaContent = GetTextInsideHtmlMetaTags(headHtml);

            // External links content
            string[] wordsInLinks = GetTextInsideExternalLinks(bodyHtml);

            var resultWords = new List<Word>();
            foreach (var word in words)
            {
                var count = 0;
                var metaCount = 0;
                var extLinkCount = 0;

                // Option 1
                if (options.First())
                {
                    count += wordsInBody.Count(wordInBody => word.Equals(wordInBody, StringComparison.OrdinalIgnoreCase));
                }

                // Option 2
                if (options.ElementAt(1))
                {
                    metaCount += wordsInAllMetaContent.Count(wordInAllMetaContent => wordInAllMetaContent.Contains(word, StringComparison.OrdinalIgnoreCase));
                }

                // Option 3
                if (options.Last())
                {
                    extLinkCount += wordsInLinks.Count(wordInLinks => wordInLinks.Contains(word, StringComparison.OrdinalIgnoreCase));
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
                _logger.LogError(e.ToString());
                throw;
            }
        }
    }
}
