using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prxSearcher
{
    class Searcher
    {
        public string url { get; set; }
        public int step { get; set; }
        public int first { get; set; }
        public splitter spltr { get; set; }
        public string srchVar { get; set; }
        public string pageVar { get; set; }
        public string regexExpOfResults { get; set; }

        /// <summary>
        /// разделитель в искомой фразе
        /// </summary>
        public enum splitter
        {
            add = 43, //+
            p20 = 37, //%
            space = 32 //space
        }

        public override string ToString()
        {
            return html_parser.Replace(url, "(?<=http(s)?://.*)/.*$", "");
        }
    }
}
