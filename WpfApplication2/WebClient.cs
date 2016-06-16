using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

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

                request.Timeout = 10000;

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

        public static bool GetViaSocks(bool SocksVer5, string Uri, string Proxy, out string html, out double TimeSpan)
        {            
            html = string.Empty;
            TimeSpan = 0;

            string _proxyUrl = html_parser.Match(Proxy, "^.*?(?=:)");
            string _proxyPort = html_parser.Match(Proxy, @"(?<=:)\d*");

            string _host = html_parser.Match(Uri.Replace("https","http"), "(?<=http://).*?(?=(/|$))");
            string _hostPort = html_parser.Match(Uri, @"(?<=:)\d*");
            _hostPort = (_hostPort == string.Empty) ? "80" : _hostPort;

            DateTime start = DateTime.Now;
            try
            {
                Socket socket = Socks5Client.Connect(SocksVer5, _proxyUrl, int.Parse(_proxyPort), _host, int.Parse(_hostPort), null, null);
                string userAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:21.0) Gecko/20100101 Firefox/21.0";
                byte[] request = Encoding.ASCII.GetBytes(String.Format("GET / HTTP/1.1\r\nHost: {0}\r\nUser-Agent: {1}\r\n\r\n", _host, userAgent));
                socket.Send(request);
                byte[] buffer = new byte[2048];
                int recv;

                while ((recv = socket.Receive(buffer, 2048, SocketFlags.None)) > 0)
                {
                    string response = Encoding.ASCII.GetString(buffer, 0, recv);
                    html += response;
                    if (!socket.Poll(1000 * 1000, SelectMode.SelectRead))
                        break;
                }
                socket.Close();

                TimeSpan = (DateTime.Now - start).TotalMilliseconds;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
