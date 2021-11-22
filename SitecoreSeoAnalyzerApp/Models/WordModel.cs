using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SitecoreSeoAnalyzerApp.Models
{
    /// <summary>
    /// Word model
    /// </summary>
    public class Word
    {
        /// <summary>
        /// Word from the text
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Number of occurrences on page content  
        /// </summary>
        public int Count { get; set; }


        /// <summary>
        /// Number of occurrences on meta tag
        /// </summary>
        public int MetaCount { get; set; }

        /// <summary>
        /// Number of occurrences on meta tag
        /// </summary>
        public int ExtLinkCount { get; set; }

        /// <summary>
        /// Constructor word
        /// </summary>
        public Word(string name, int count = 0, int metaCount = 0, int extLinkCOunt = 0)
        {
            Name = name;
            Count = count;
            MetaCount = metaCount;
            ExtLinkCount = extLinkCOunt;
        }
    }
}
