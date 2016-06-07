using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace prxSearcher
{
    class ProxiesList:IDisposable,IEnumerable
    {
        //-----------------------------members---------------------------------
        /// <summary>
        /// searchers
        /// </summary>
        private List<Searcher> mSearchers;
        /// <summary>
        /// how many proxies needed
        /// </summary>
        private int mPrxsCountNeed;
        /// <summary>
        /// loading threads
        /// </summary>
        private ProxySearcher[] mProxyLoadThreads;
        /// <summary>
        /// Count of searching threads
        /// </summary>
        private int mThreadsCount;
        /// <summary>
        /// The thread that waiting for the end of searching
        /// </summary>
        private Thread mWaiter;
        /// <summary>
        /// Current count of found proxies
        /// </summary>
        private int mCurrentCount;
        /// <summary>
        /// Status of searhing
        /// </summary>
        public bool mIsRun { get; set; }        
        /// <summary>
        /// The dictionary that contain proxies list
        /// </summary>
        public Dictionary<string, Proxy> mPrxsDic;
        /// <summary>
        /// For display on DataGrid
        /// </summary>
        public Proxy[] mPrxsArray;
        public int mProgressValue;
        public string mStatus;

        //public delegate void ChangedEventHandler(object sender, EventArgs e);
        public event EventHandler Changed;

        //-----------------------------methods---------------------------------
        public void Dispose()
        {
            mWaiter.Abort();
            
            for (int i = 0; i < mSearchers.Count; i++)
            {
                try
                {
                    mProxyLoadThreads[i].StopLoading();
                }
                catch (Exception) { }
            }
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="threads">Need count of proxies</param>
        /// <param name="searcher">search-system</param>
        public ProxiesList(int Threads, int Count)
        {
            mThreadsCount = Threads;
            mPrxsCountNeed = Count;
            mPrxsDic = new Dictionary<string, Proxy>();
            mSearchers = new List<Searcher>();

            mPrxsArray = new Proxy[] { };
            mCurrentCount = 0;

            mSearchers.Add(new Searcher()
            {
                url = "http://www.google.com/search?",
                step = 10,
                first = 0,
                spltr = Searcher.splitter.p20,
                srchVar = "q",
                pageVar = "start",
                regexExpOfResults = "(?<=a href=\"/url\\?q=)http(s)?://[^\"]+"
            });

            mSearchers.Add(new Searcher()
            {
                url = "http://yandex.ru/yandsearch?",
                step = 1,
                first = 0,
                spltr = Searcher.splitter.p20,
                srchVar = "text",
                pageVar = "p",
                regexExpOfResults = "(?<=title-link\" href=\")http(s)?://[^\"]+"
            });

            mSearchers.Add(new Searcher()
            {
                url = "http://nova.rambler.ru/search?",
                step = 1,
                first = 1,
                spltr = Searcher.splitter.add,
                srchVar = "query",
                pageVar = "page",
                regexExpOfResults = "(?<=<span class=\"b-serp__list_item_info_domain\">)[^<>\\[\\]]+(?=</span>)"
            });            

            mSearchers.Add(new Searcher()
            {
                url = "http://www.bing.com/search?",
                step = 10,
                first = 1,
                spltr = Searcher.splitter.add,
                srchVar = "q",
                pageVar = "first",
                regexExpOfResults = "(?<=<cite>).*?(?=</cite>)"
            });

            mSearchers.Add(new Searcher()
            {
                url = "https://search.yahoo.com/search?",
                step = 10,
                first = 1,
                spltr = Searcher.splitter.add,
                srchVar = "p",
                pageVar = "b",
                regexExpOfResults = "(?<=wr-bw\">).*?(?=</span>)"
            });

            mSearchers.Add(new Searcher()
            {
                url = "http://go.mail.ru/search?",
                step = 10,
                first = 0,
                spltr = Searcher.splitter.add,
                srchVar = "q",
                pageVar = "sf",
                regexExpOfResults = "(?<=serp__link\" href=\").*?(?=\")"
            });
        }
        /// <summary>
        /// Start parsing
        /// </summary>
        /// <param name="phrase">Search phrase ex.:proxy list</param>
        public void GetProxiesList(string phrase)
        {
            mProxyLoadThreads = new ProxySearcher[mThreadsCount];
            int idOfSearcher = 0;

            for (int i = 0; i < mThreadsCount; i++)
            {
                foreach (var sr in mSearchers)
                {
                    int pageNum = sr.first + sr.step * (i - mSearchers.Count);                    
                    mProxyLoadThreads[i] = new ProxySearcher(i, idOfSearcher, phrase, pageNum, ref mSearchers, ref mPrxsDic, mPrxsCountNeed);
                    idOfSearcher++;
                }
                idOfSearcher = 0;                
            }
            mWaiter = new Thread(new ThreadStart(IsLoaded));
            mWaiter.Start();
            mIsRun = true;
            OnChanged(null);
        }    
        /// <summary>
        /// Get active searching threads count
        /// </summary>
        /// <returns></returns>
        private int GetActiveSearchersCount()
        {
            int n = 0;
            for (int i=0;i<mProxyLoadThreads.Length;i++)
            {
                n += mProxyLoadThreads[i].IsRun() ? 1 : 0;
            }
            return n;
        }
        /// <summary>
        /// work method of waiter thread
        /// </summary>
        private void IsLoaded()
        {
            bool end = false;          
            while (!end )
            {
                end = false;

                end = (GetActiveSearchersCount() > 0) ? false : true;
                                
                if(mCurrentCount != mPrxsDic.Count)
                {
                    mCurrentCount = mPrxsDic.Count;
                    OnChanged(EventArgs.Empty);
                }
                if(end == true)
                {
                    mIsRun = false;
                    OnChanged(EventArgs.Empty);
                }
                Thread.Sleep(200);
            }            
        }
        /// <summary>
        /// delete proxy from dictionary
        /// </summary>
        /// <param name="Value"></param>
        public void Remove(object Value)
        {
            Proxy p = (Proxy)Value;
            if (mPrxsDic.ContainsKey(p.adress))
            {
                mPrxsDic.Remove(p.adress);
                OnChanged(EventArgs.Empty);
            }
        }
        /// <summary>
        /// stop searching of proxies
        /// </summary>
        public void StopProxiesLoading()
        {
            for (int i = 0; i < mProxyLoadThreads.Length; i++)
            {
                mProxyLoadThreads[i].StopLoading();                
            }
        }

        private void OnChanged(EventArgs e)
        {
            mPrxsArray = new Proxy[mPrxsDic.Count];

            int i = 0;
            foreach (KeyValuePair<string, Proxy> p in mPrxsDic)
            {
                mPrxsArray[i] = p.Value;
                i++;
            }

            mProgressValue = (!mIsRun) ? 0 : Convert.ToInt32(Math.Round((double)i * 100 / mPrxsCountNeed, 0));
            mStatus = (mIsRun) ? String.Format("Active threads: {0}; Loaded: {1}", GetActiveSearchersCount(), i) : String.Format("Done; Loaded: {0}", i);

            if (Changed != null)
                Changed(this, e);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        class Enumerator:IEnumerator
        {
            private ProxiesList mPL;
            private int mPos;

            public Enumerator(ProxiesList pl)
            {
                mPL = pl;
                mPos = -1;
            }

            public object Current
            {
                get
                {
                    return mPL.mPrxsArray[mPos];
                }
            }

            public bool MoveNext()
            {
                if (mPos == mPL.mPrxsArray.Length - 1)
                    return false;
                mPos++;
                return true;
            }

            public void Reset()
            {
                mPos = -1;
            }
        }
    }
}
