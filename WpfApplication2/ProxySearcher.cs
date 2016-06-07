using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace prxSearcher
{
    class ProxySearcher
    {
        private Thread mT;
        private bool mIsRun { get; set; }
        private int mIdOfSearcher;
        private string mSearchPhrase;
        private int mPage;
        List<Searcher> mSearchersList;
        private Dictionary<string, Proxy> mPrxsDic;
        private int mPrxsCountNeed;


        //methods
        public ProxySearcher(int NameOfThread, int IdOfSearcher, string SearchPhrase, int StartFromPage, ref List<Searcher> SearchersList, ref Dictionary<string, Proxy> PrxsDic, int PrxsCountNeed)
        {
            mIdOfSearcher = IdOfSearcher;
            mSearchPhrase = SearchPhrase;
            mPage = StartFromPage;
            mSearchersList = SearchersList;
            mPrxsDic = PrxsDic;
            mPrxsCountNeed = PrxsCountNeed;
            
            mT = new Thread(new ThreadStart(ProxyToAssemble));
            mT.Name = NameOfThread.ToString();
            mIsRun = true;
            mT.Start();
        }

        public bool IsRun()
        {
            return mIsRun;
        }

        /// <summary>
        /// Search proxies and then parse them
        /// </summary>
        /// <param name="Prm">Id of thread</param>
        private void ProxyToAssemble()
        {
            StringBuilder txtQuery = new StringBuilder();
            txtQuery.Append(mSearchersList[mIdOfSearcher].url);
            txtQuery.Append("&");
            txtQuery.Append(mSearchersList[mIdOfSearcher].srchVar);
            txtQuery.Append("=");
            txtQuery.Append(mSearchPhrase.Replace(" ", ((char)mSearchersList[mIdOfSearcher].spltr).ToString()));
            txtQuery.Append("&");
            txtQuery.Append(mSearchersList[mIdOfSearcher].pageVar);
            txtQuery.Append("=");
            
            try
            {
                while (mPrxsDic.Count < mPrxsCountNeed && mIsRun)
                {
                    string html = "";
                    string uri = txtQuery.ToString() + mPage.ToString();
                    if (Get(uri, string.Empty, out html))
                    {
                        if (html.ToLower().Contains("captcha"))
                            StopLoading();
                        ParseProxies(html_parser.Matches(html, mSearchersList[mIdOfSearcher].regexExpOfResults));
                    }
                    mPage += mSearchersList[mIdOfSearcher].step;
                    Thread.Sleep(200);
                }
            }
            catch(Exception ex) { }
            finally
            { 
                mIsRun = false;
            }
        }

        /// <summary>
        /// Запрос методом GET
        /// </summary>
        private static bool Get(string Uri,string Proxy, out string Html)
        {
            if (Uri == "")
            {
                Html = "";
                return false;
            }
            Html = "";

            try
            {                
                if (Uri.Length > 0 && !html_parser.Test(Uri,"^https?://.*"))
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

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                        return false;

                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    Html = reader.ReadToEnd();

                    return true;
                }
            }
            catch(Exception)
            {                
                return false;
            }
        }

        private void ParseProxies(string[] Urls)
        {
            foreach (string s in Urls)
            {                
                if (mPrxsDic.Count < mPrxsCountNeed && mIsRun)
                {
                    string html = "";
                    if (Get(html_parser.ClearUrl(s), "", out html))
                    {
                        string[] proxies = html_parser.Matches(html, @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}.\d{2,5}");
                        if (proxies.Length > 0)
                        {
                            foreach (string p in proxies)
                            {
                                if (p != "")
                                {
                                    if (!mPrxsDic.ContainsKey(p))
                                    {
                                        Proxy newProxy = new Proxy();
                                        newProxy.adress = p;
                                        Add(newProxy);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    mIsRun = false;
                    return;
                }
            }
        }

        private void Add(object Value)
        {
            lock (mPrxsDic)
            {
                Proxy p = (Proxy)Value;
                if (!mPrxsDic.ContainsKey(p.adress))
                {
                    mPrxsDic.Add(p.adress, p);
                }

                mIsRun = (mPrxsDic.Count >= mPrxsCountNeed) ? false : true;
            }
        }

        public void StopLoading()
        {
            mIsRun = false;
            mT.Abort();                        
        }
    }
}
