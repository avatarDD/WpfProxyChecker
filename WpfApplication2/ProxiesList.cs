using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace prxSearcher
{
    class ProxiesList : IDisposable, IEnumerable
    {
        //-----------------------------members---------------------------------
        /// <summary>
        /// searchers
        /// </summary>
        private List<Searcher> mSearchers;
        /// <summary>
        /// Dictionary of used numbers page number variable of searchers
        /// </summary>
        private Dictionary<Searcher, Dictionary<int, bool>> mCurrentSearchPageOfSearcherDic;
        /// <summary>
        /// how many proxies needed
        /// </summary>
        private int mPrxsCountNeed;
        /// <summary>
        /// loading threads
        /// </summary>
        private ProxySearcher[] mProxyLoadThreads;
        private TestProxies[] mTestProxiesThreads;
        /// <summary>
        /// Count of searching threads
        /// </summary>
        public int mThreadsCount;
        /// <summary>
        /// Status of searhing
        /// </summary>
        public bool mIsRunFinding { get; set; }
        public bool mIsRunTesting { get; set; }
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
        public bool mPrxsFound;
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
        /// <param name="Threads">Count of searching threads</param>
        /// <param name="Count">count of needed proxies</param>
        /// <param name="SearchersList">list of searcher systems</param>
        /// <param name="Proxy">searching via proxy?</param>
        /// <param name="SearchPhrase">seach phrase</param>
        public ProxiesList(int Threads, int Count, List<Searcher> SearchersList, string Proxy, string SearchPhrase)
        {
            mPrxsDic = new Dictionary<string, Proxy>();
            mThreadsCount = Threads;
            mPrxsCountNeed = Count;
            mSearchers = SearchersList;
            mProxy = Proxy;
            mSearchPhrase = SearchPhrase;
            mPrxsArray = new Proxy[] { };
            mCurrentSearchPageOfSearcherDic = new Dictionary<Searcher, Dictionary<int, bool>>() { };
        }
        /// <summary>
        /// Start parsing
        /// </summary>
        /// <param name="phrase">Search phrase ex.:proxy list</param>
        public void GetProxiesList()
        {
            mProxyLoadThreads = new ProxySearcher[mThreadsCount];
            mIsRunFinding = true;

            for (int i = 0; i < mThreadsCount; i++)
            {
                foreach (var sr in mSearchers)
                {
                    int pageNum = GetNewPageNumber(sr);
                    mProxyLoadThreads[i] = new ProxySearcher(this, i, sr, mSearchPhrase, pageNum, ref mSearchers, ref mPrxsDic, mPrxsCountNeed, mProxy);
                    mProxyLoadThreads[i].mKilled += new ProxySearcher.mKilledEventHandler(UpdateProxyLoadThreadsList);
                    mProxyLoadThreads[i].mPrxsLstUpdated += new EventHandler(OnChanged);
                }
            }

            OnChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Test proxies Dictionary
        /// </summary>
        public void TestProxiesDictionary(int Threads, string Target, string RegexCountry)
        {
            mThreadsCount = Threads;
            mIsRunTesting = true;

            mTestProxiesThreads = new TestProxies[mThreadsCount];

            int xStartFrom = 0;
            int xLength = mPrxsArray.Length / mThreadsCount;
            int xRemainder = mPrxsArray.Length % mThreadsCount;

            if (xLength == 0)
                return;

            for (int i = 0; i < mThreadsCount; i++)
            {
                if (i + 1 >= mThreadsCount)
                {
                    xLength += xRemainder;
                }
                Proxy[] partOfArray = new Proxy[xLength];
                Array.Copy(mPrxsArray, xStartFrom, partOfArray, 0, xLength);
                xStartFrom += xLength;
                mTestProxiesThreads[i] = new TestProxies(ref mPrxsDic, Target, partOfArray, RegexCountry);
                mTestProxiesThreads[i].mTstDead += new EventHandler(TestProxiesThreadsListUpdate);
                mTestProxiesThreads[i].mPrxsLstUpdated += new EventHandler(OnChanged);
            }

            OnChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Get new not repeated page number for variable of Searcher
        /// </summary>
        /// <param name="searcher">Current searcher</param>
        /// <returns></returns>
        public int GetNewPageNumber(Searcher searcher)
        {
            int result = searcher.first;

            if (!mCurrentSearchPageOfSearcherDic.ContainsKey(searcher))
            {
                foreach (Searcher s in mSearchers)
                    mCurrentSearchPageOfSearcherDic.Add(s, new Dictionary<int, bool> { });
            }
            Dictionary<int, bool> dicOfPages = mCurrentSearchPageOfSearcherDic[searcher];

            lock (mCurrentSearchPageOfSearcherDic)
            {
                for (;;)
                {
                    if (dicOfPages.ContainsKey(result))
                    {
                        result += searcher.step;
                    }
                    else
                    {
                        mCurrentSearchPageOfSearcherDic[searcher].Add(result, true);
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
        private void UpdateProxyLoadThreadsList(object sender, KilledEventArgs e)
        {
            if (mThreadsCount == 0)
                return;

            if (mThreadsCount > 0)
            {
                ProxySearcher[] a = new ProxySearcher[mThreadsCount - 1];

                int k = 0;
                for (int i = 0; i < mThreadsCount; i++)
                {
                    if (mProxyLoadThreads.Length < mThreadsCount)
                    {
                        mThreadsCount = mProxyLoadThreads.Length;
                        OnChanged(this, EventArgs.Empty);
                        return;
                    }
                    if (mProxyLoadThreads[i] == null)
                        break;
                    if (mProxyLoadThreads[i].mId != e.mParam)
                    {
                        if (i - k < a.Length)
                        {
                            a[i - k] = mProxyLoadThreads[i];
                        }
                        else
                        {
                            mThreadsCount = mProxyLoadThreads.Length;
                            OnChanged(this, EventArgs.Empty);
                            return;
                        }
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
                return;
            }
            OnChanged(this, EventArgs.Empty);
        }

        private void TestProxiesThreadsListUpdate(object sender, EventArgs e)
        {
            mThreadsCount = mTestProxiesThreads.Length;
            TestProxies[] a = new TestProxies[mThreadsCount - 1];
            int k = 0;
            for (int i = 0; i < mThreadsCount; i++)
            {
                if (mTestProxiesThreads[i] == null)
                {
                    continue;
                }
                if (mTestProxiesThreads[i].mIsRun)
                {
                    a[k] = mTestProxiesThreads[i];
                    k++;
                }
            }
            mTestProxiesThreads = a;
            mThreadsCount = mTestProxiesThreads.Length;

            OnChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// stop searching of proxies
        /// </summary>
        public void StopProxiesWorkers()
        {
            if (mIsRunFinding)
            {
                for (int i = 0; i < mProxyLoadThreads.Length; i++)
                {
                    mProxyLoadThreads[i].StopLoading();
                }
                while(mThreadsCount != 0)
                {
                    Thread.Sleep(200);
                }
                mIsRunFinding = false;
            }
            if (mIsRunTesting)
            {
                for (int i = 0; i < mTestProxiesThreads.Length; i++)
                {
                    mTestProxiesThreads[i].StopTesting();
                }
                while (mThreadsCount != 0)
                {
                    Thread.Sleep(200);
                }
                mIsRunTesting = false;
            }

            OnChanged(this, EventArgs.Empty);
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

            mPrxsFound = (i>0)?true:false;

            if (mIsRunFinding)
            {
                if (Convert.ToInt32(Math.Round((double)i * 100 / mPrxsCountNeed, 0)) >= 100 && mThreadsCount == 0)
                {
                    mIsRunFinding = false;
                }
                mProgressValue = (!mIsRunFinding) ? 0 : Convert.ToInt32(Math.Round((double)i * 100 / mPrxsCountNeed, 0));
                mStatus = (mIsRunFinding) ? string.Format("Active threads: {0}; Loaded: {1}", mThreadsCount, i) : String.Format("Done; Loaded: {0}", i);
            }
            if (mIsRunTesting)
            {
                i = (i >= mPrxsCountNeed) ? mPrxsCountNeed : i;
                if (mThreadsCount == 0)
                {
                    mIsRunTesting = false;
                }                
                mStatus = (mIsRunTesting) ? string.Format("Active threads: {0}; Found: {1}", mThreadsCount, i) : String.Format("Done; Found: {0}", i);
            }

            if (mPrxsDic.Count > mPrxsCountNeed && mIsRunFinding)
            {
                StopProxiesWorkers();
            }

            Changed(this, EventArgs.Empty);            
        }

        public void SaveResultToFile(string path)
        {
            using (StreamWriter sw = new StreamWriter(path))
            {
                try
                {
                    foreach (KeyValuePair<string, Proxy> i in mPrxsDic)
                    {
                        sw.WriteLine(i.Value.adress);
                    }
                }
                catch (Exception)
                { }
                finally
                {
                    if(sw!=null)
                        sw.Close();
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        class Enumerator : IEnumerator
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
