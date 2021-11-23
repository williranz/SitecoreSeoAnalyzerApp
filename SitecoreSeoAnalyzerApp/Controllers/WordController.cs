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
    /// html body enum 
    /// </summary>
    public enum HtmlPart
    {
        Body,
        Head
    }
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
        /// Regex for alpha numeric and space
        /// </summary>
        private static readonly Regex _regexTextInput = new Regex(HtmlPartConst.RegexAlphaNumeric);

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
            // clean text input from all non alphanumeric except spaces
            string cleanText = CleanTextInput(text);
            
            // split text into words
            List<string> grossWords = SplitStringIntoWords(cleanText);

            // remove stop-words inside words 
            IEnumerable<string> cleanWords = RemoveStopWords(grossWords);

            // Make each word unique, no duplicate
            List<string> cleanDistinctWords = cleanWords.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            // Process word for SEO analysis against page content 
            var options = new List<bool>() { opt1, opt2, opt3 };
            List<Word> finalResult = ProcessSeoAnalysis(cleanDistinctWords, url, options);
            
            // Return result in Json
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
            // filter out all non alphanumeric characters
            string output = _regexTextInput.Replace(text, string.Empty);
            return output;
        }

        /// <summary>
        /// Get html body from full html document
        /// </summary>
        /// <param name="html"></param>
        /// <returns>Html body element</returns>
        static string getHtmlPart(string html, HtmlPart htmlPart )
        {
            var startTag = string.Empty;
            var endTag = string.Empty;
            if (htmlPart == HtmlPart.Body)
            {
                startTag = HtmlPartConst.HtmlBodyTagStart;
                endTag = HtmlPartConst.HtmlBodyTagEnd;
            }
            else
            {
                startTag = HtmlPartConst.HtmlHeadTagStart; ;
                endTag = HtmlPartConst.HtmlHeadTagEnd; ;
            }

            // get start and end html part of the page
            int startTagIndex = html.IndexOf(startTag, StringComparison.OrdinalIgnoreCase);
            int endTagIndex = html.IndexOf(endTag, StringComparison.OrdinalIgnoreCase) + endTag.Length;

            // calculate index of part element
            int partLength = endTagIndex - startTagIndex;

            // get only part element
            string htmlPartResult = html.Substring(startTagIndex, partLength);

            return htmlPartResult;
        }

        /// <summary>
        /// Get text inside html body
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        static string[] getTextInsideHtmlBody(string html)
        {
            // split all html lines into separate string
            string[] bodyElementLines = html.Split(Environment.NewLine);

            List<string> bodyElementList = new List<string>();
            
            foreach (var bodyElementLine in bodyElementLines)
            {
                // trim white spaces and take non empty line only
                string bodyElementLineTrimmed = bodyElementLine.Trim();
                
                if (!string.IsNullOrWhiteSpace(bodyElementLineTrimmed))
                {
                    bodyElementList.Add(bodyElementLineTrimmed);
                }
            }

            // combine all line and break down into separate word for comparison 
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
            // split all html lines into separate string
            string[] elementLines = html.Split(Environment.NewLine);
            List<string> allMetaContent = new List<string>();
            foreach (var elementLine in elementLines)
            {
                // if contain meta tag
                if (elementLine.IndexOf(HtmlPartConst.MetaTagStart, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    // Trim white spaces
                    string elementLineTrimmed = elementLine.Trim();

                    // get all meta content in 1 line
                    string[] metaLines = elementLineTrimmed.Split(HtmlPartConst.MetaTagStart);

                    // extract content from meta tag
                    foreach (var metaLine in metaLines)
                    {
                        if (metaLine.IndexOf(HtmlPartConst.MetaContent + HtmlPartConst.DoubleQuote, StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            int startMetaTagIndex = metaLine.IndexOf(HtmlPartConst.MetaContent + HtmlPartConst.DoubleQuote, StringComparison.OrdinalIgnoreCase) + (HtmlPartConst.MetaContent + HtmlPartConst.DoubleQuote).Length;
                            string metaContent = metaLine.Substring(startMetaTagIndex);
                            int endMetaTagIndex = metaContent.IndexOf(HtmlPartConst.DoubleQuote, StringComparison.CurrentCulture) - HtmlPartConst.DoubleQuote.Length;
                            metaContent = metaContent.Substring(0, endMetaTagIndex);
                            allMetaContent.Add(metaContent);
                        }
                    }
                }
            }

            // all word inside content meta tags combined and break down into separate words
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
            // split all html lines into separate string
            string[] elementLines = html.Split(Environment.NewLine);
            
            List<string> allLinksLinesRaw = new List<string>();
            foreach (var elementLine in elementLines)
            {
                // if contains external link
                if (elementLine.IndexOf(HtmlPartConst.LinkTagStart + HtmlPartConst.DoubleQuote + HtmlPartConst.Http, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    // trim white spaces
                    string elementLineTrimmed = elementLine.Trim();
                    allLinksLinesRaw.Add(elementLineTrimmed);
                }
            }

            List<string> allLinksLines = new List<string>();
            foreach (var linksLine in allLinksLinesRaw)
            {
                // remove all html external link tag
                int startLinkTextIndex = linksLine.IndexOf(HtmlPartConst.LinkTagStart + HtmlPartConst.DoubleQuote + HtmlPartConst.Http, StringComparison.OrdinalIgnoreCase);
                int endLinkTextIndex = linksLine.IndexOf(HtmlPartConst.CloseTag, StringComparison.CurrentCulture) + HtmlPartConst.CloseTag.Length;
                string linkTextRemoved = linksLine.Remove(startLinkTextIndex, endLinkTextIndex);
                allLinksLines.Add(linkTextRemoved);
            }

            // add all external link text after their html external link tag removed
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
            // split by space
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
            // get stop words from file and remove stop words from input words 
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
            // Get web client data
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

            // Get words from web client body element
            string bodyHtml = getHtmlPart(rawTextContent, HtmlPart.Body);
            string[] wordsInBody = getTextInsideHtmlBody(bodyHtml);

            // Get words from web client meta tags
            string headHtml = getHtmlPart(rawTextContent, HtmlPart.Head);
            string[] wordsInAllMetaContent = GetTextInsideHtmlMetaTags(headHtml);

            // Get data from elements with external links
            string[] wordsInLinks = GetTextInsideExternalLinks(bodyHtml);

            // Analyze result based on selection analysis option(s)
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

                // add result word by word
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
                // get list of stop word from csv file
                var stopWordsPath = Path.Combine(_hostingEnvironment.ContentRootPath, @"wwwroot\StopWords.csv");
                using var reader = new StreamReader(stopWordsPath);
                var listStopWords = new List<string>();
                while (!reader.EndOfStream)
                {
                    // read each line
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
