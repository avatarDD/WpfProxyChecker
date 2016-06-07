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
        public int mThreadsCount;
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
        public event EventHandler Changed;                

        //-----------------------------methods---------------------------------
        public void Dispose()
        {
            try
            {
                for (int i = 0; i < mProxyLoadThreads.Length; i++)
                {
                    mProxyLoadThreads[i].StopLoading();
                }
            }
            catch (Exception) { }
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
                    mProxyLoadThreads[i].mKilled += new ProxySearcher.mKilledEventHandler(UpdateProxyLoadThreadsList);
                    mProxyLoadThreads[i].mPrxsLstUpdated += new EventHandler(OnChanged);
                    idOfSearcher++;
                }
                idOfSearcher = 0;                
            }
            mIsRun = true;
            OnChanged(this,EventArgs.Empty);
        }           
               
        public void UpdateProxyLoadThreadsList(object sender, KilledEnentArgs e)
        {
            if (!mIsRun)
                return;

            ProxySearcher[] a = new ProxySearcher[mProxyLoadThreads.Length - 1];
            int k=0;

            for (int i = 0; i < mProxyLoadThreads.Length; i++)
            {
                if (mProxyLoadThreads[i].mId != e.mParam)
                {
                    a[i-k] = mProxyLoadThreads[i];
                }
                else
                {
                    k = 1;                    
                    continue;
                }
            }
            mProxyLoadThreads = a;
            mThreadsCount = mProxyLoadThreads.Length;
            OnChanged(this, EventArgs.Empty);
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
                OnChanged(this,EventArgs.Empty);
            }
        }
        /// <summary>
        /// stop searching of proxies
        /// </summary>
        public void StopProxiesLoading()
        {
            while (mIsRun)
            {
                for (int i = 0; i < mProxyLoadThreads.Length; i++)
                {
                    mProxyLoadThreads[i].StopLoading();
                }
                mIsRun = (mProxyLoadThreads.Length > 0) ? true:false;
            }
        }

        private void OnChanged(object sender, EventArgs e)
        {
            mPrxsArray = new Proxy[mPrxsDic.Count];

            int i = 0;
            foreach (KeyValuePair<string, Proxy> p in mPrxsDic)
            {
                mPrxsArray[i] = p.Value;
                i++;
            }

            mProgressValue = (!mIsRun) ? 0 : Convert.ToInt32(Math.Round((double)i * 100 / mPrxsCountNeed, 0));
            mStatus = (mIsRun) ? String.Format("Active threads: {0}; Loaded: {1}", mThreadsCount, i) : String.Format("Done; Loaded: {0}", i);

            if (Changed != null)
                Changed(this, EventArgs.Empty);

            if(mPrxsDic.Count > mPrxsCountNeed)
            {
                StopProxiesLoading();
                mIsRun = false;
            }
            else if(mThreadsCount == 0)
            {
                mIsRun = false;
            }
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
