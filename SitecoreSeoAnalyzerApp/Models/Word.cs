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
        /// Number of occurrences  
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Constructor word
        /// </summary>
        public Word(string name, int count)
        {
            Name = name;
            Count = count;
        }
    }
}
