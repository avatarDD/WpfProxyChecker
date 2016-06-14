using System;
using System.IO;
using System.Net;

namespace prxSearcher
{
    static class Web_Client
    {
        public static bool Get(string Uri, string Proxy, out string Html, out double TimeSpan)
        {
            TimeSpan = 0;

            if (Uri == "")
            {
                Html = "";
                return false;
            }
            Html = "";

            try
            {
                if (Uri.Length > 0 && !html_parser.Test(Uri, "^https?://.*"))
                {
                    Uri = "http://" + Uri;
                }

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Uri);

                if (Proxy != "")
                {
                    request.Proxy = new WebProxy(Proxy);
                }
                //---------
                //request.Proxy = new WebProxy("127.0.0.1:3128");
                //---------
                request.Timeout = 8000;

                DateTime start = DateTime.Now;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                        return false;

                    TimeSpan = (DateTime.Now - start).TotalMilliseconds;

                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    Html = reader.ReadToEnd();

                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
