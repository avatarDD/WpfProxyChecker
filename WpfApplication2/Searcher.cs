using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;

namespace prxSearcher
{
    [DataContract]
    class Searcher
    {
        [DataMember]
        public string url { get; set; }
        [DataMember]
        public int step { get; set; }
        [DataMember]
        public int first { get; set; }
        [DataMember]
        public splitter spltr { get; set; }
        [DataMember]
        public string srchVar { get; set; }
        [DataMember]
        public string pageVar { get; set; }
        [DataMember]
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
