using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace prxSearcher
{
    [DataContract]
    class Target
    {
        [DataMember]
        public string mAdress { get; set; }
        [DataMember]
        public string mRegexContry { get; set; }

        public override string ToString()
        {
            return html_parser.Replace(mAdress, "(?<=http(s)?://.*)/.*$", "");
        }
    }
}
