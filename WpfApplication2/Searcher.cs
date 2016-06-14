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
        public string spltr { get; set; }
        [DataMember]
        public string srchVar { get; set; }
        [DataMember]
        public string pageVar { get; set; }
        [DataMember]
        public string regexExpOfResults { get; set; }

        public override string ToString()
        {            
            return html_parser.Replace(url, "(?<=http(s)?://.*)/.*$", "");
        }
    }
}
