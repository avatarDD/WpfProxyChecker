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
        
        private Dictionary<Searcher,Dictionary<int,bool>> mCurrentSearchPageOfSearcher;
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
        private string mProxy;
        private string mSearchPhrase;
        /// <summary>
        /// For display on DataGrid
        /// </summary>
        public Proxy[] mPrxsArray;
        public int mProgressValue;
        public string mStatus;        
        public event EventHandler Changed;  
        /// <summary>
        /// Calc number of callings of onChange (for slowly computers redraw only each 10th iteration and last)
        /// </summary>
        private int mCallId;

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
        /// <param name="Threads">Count of searching threads</param>
        /// <param name="Count">count of needed proxies</param>
        /// <param name="SearchersList">list of searcher systems</param>
        /// <param name="Proxy">searching via proxy?</param>
        /// <param name="SearchPhrase">seach phrase</param>
        public ProxiesList(int Threads, int Count, List<Searcher> SearchersList, string Proxy, string SearchPhrase)
        {
            mCallId = 0;
            mPrxsDic = new Dictionary<string, Proxy>();
            mThreadsCount = Threads;
            mPrxsCountNeed = Count;            
            mSearchers = SearchersList;
            mProxy = Proxy;
            mSearchPhrase = SearchPhrase;
            mPrxsArray = new Proxy[] { };
            mCurrentSearchPageOfSearcher = new Dictionary<Searcher, Dictionary<int, bool>>() { };
        }
        /// <summary>
        /// Start parsing
        /// </summary>
        /// <param name="phrase">Search phrase ex.:proxy list</param>
        public void GetProxiesList()
        {
            mProxyLoadThreads = new ProxySearcher[mThreadsCount];

            for (int i = 0; i < mThreadsCount; i++)
            {
                foreach (var sr in mSearchers)
                {
                    int pageNum = GetNewPageNumber(sr);
                    mProxyLoadThreads[i] = new ProxySearcher(this, i, sr, mSearchPhrase, pageNum, ref mSearchers, ref mPrxsDic, mPrxsCountNeed);
                    mProxyLoadThreads[i].mKilled += new ProxySearcher.mKilledEventHandler(UpdateProxyLoadThreadsList);
                    mProxyLoadThreads[i].mPrxsLstUpdated += new EventHandler(OnChanged);
                }               
            }
            mIsRun = true;
            OnChanged(this,EventArgs.Empty);
        }
        /// <summary>
        /// Get new not repeated page number for variable of Searcher
        /// </summary>
        /// <param name="searcher">Current searcher</param>
        /// <returns></returns>
        public int GetNewPageNumber(Searcher searcher)
        {
            int result = searcher.first;
            
            if (!mCurrentSearchPageOfSearcher.ContainsKey(searcher))
            {
                foreach(Searcher s in mSearchers)
                    mCurrentSearchPageOfSearcher.Add(s, new Dictionary<int, bool> { });
            }
            Dictionary<int, bool> dicOfPages = mCurrentSearchPageOfSearcher[searcher];

            lock (mCurrentSearchPageOfSearcher)
            {
                for (; ; )
                {
                    if (dicOfPages.ContainsKey(result))
                    {
                        result += searcher.step;
                    }
                    else
                    {
                        mCurrentSearchPageOfSearcher[searcher].Add(result, true);
                        return result;
                    }
                }
            }
        }

        /// <summary>
        /// Update list of active searching threads
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            //-----for slowly computers redraw only each 10th iteration and last
            if (mCallId % 10 != 0 && mCallId < mPrxsDic.Count)
            {
                mCallId++;
                return;
            }
            mCallId++;
            //-------------------------

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
