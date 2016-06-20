using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Threading;

namespace prxSearcher
{
    sealed class ProxiesList : IDisposable, IEnumerable
    {
        //-----------------------------members---------------------------------
        /// <summary>
        /// searchers
        /// </summary>
        private List<Searcher> mSearchers;
        /// <summary>
        /// list of targets
        /// </summary>
        private List<Target> mTargets;
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
        private List <ProxySearcher> mProxyLoadThreads;
        private List <TestProxies> mTestProxiesThreads;
        /// <summary>
        /// Count of searching threads
        /// </summary>
        public int mThreadsCountFind;
        /// <summary>
        /// count of testing threads
        /// </summary>
        public int mThreadsCountTest;
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
        private int mTimeOutFind;
        private int mTimeOutTest;
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
            StopProxiesWorkers();
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Threads">Count of searching threads</param>
        /// <param name="Count">count of needed proxies</param>
        /// <param name="SearchersList">list of searcher systems</param>
        /// <param name="Proxy">searching via proxy?</param>
        /// <param name="SearchPhrase">seach phrase</param>
        public ProxiesList(int Threads, int Count, List<Searcher> SearchersList, string Proxy, string SearchPhrase, int TimeOut)
        {
            mPrxsDic = new Dictionary<string, Proxy>();
            mThreadsCountFind = Threads;
            mPrxsCountNeed = Count;
            mSearchers = SearchersList;
            mProxy = Proxy;
            mSearchPhrase = SearchPhrase;
            mTimeOutFind = TimeOut;
            mPrxsArray = new Proxy[] { };
            mCurrentSearchPageOfSearcherDic = new Dictionary<Searcher, Dictionary<int, bool>>() { };
        }
        /// <summary>
        /// Start parsing
        /// </summary>
        /// <param name="phrase">Search phrase ex.:proxy list</param>
        public void GetProxiesList()
        {
            mProxyLoadThreads = new List<ProxySearcher>();
            mIsRunFinding = true;
            int srchrId = 0;
            for (int i = 0; i < mThreadsCountFind; i++)
            {
                int pageNum = GetNewPageNumber(mSearchers[srchrId]);
                var prsr = new ProxySearcher(this, mSearchers[srchrId], mSearchPhrase, pageNum, ref mSearchers, ref mPrxsDic, mPrxsCountNeed, mProxy, mTimeOutFind);
                prsr.mKilled += new EventHandler(UpdateProxyLoadThreadsList);
                prsr.mPrxsLstUpdated += new EventHandler(OnChanged);
                prsr.Start();
                mProxyLoadThreads.Add(prsr);
                srchrId = (srchrId + 1 > mSearchers.Count - 1) ? 0 : srchrId + 1;
            }

            OnChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Test proxies Dictionary
        /// </summary>
        public void TestProxiesDictionary(int Threads, List<Target> TargetsList, int TimeOut)
        {
            while(mThreadsCountFind > 0)
            {
                Thread.Sleep(100);
            }
            mTargets = TargetsList;
            mTimeOutTest = TimeOut;
            mThreadsCountTest = Threads;
            mIsRunTesting = true;

            mTestProxiesThreads = new List<TestProxies>();

            int xStartFrom = 0;
            int xLength = mPrxsArray.Length / Threads;
            int xRemainder = mPrxsArray.Length % Threads;

            if (xLength == 0)
                return;

            int idTarget = 0;
            for (int i = 0; i < Threads; i++)
            {
                if (i + 1 >= Threads)
                {
                    xLength += xRemainder;
                }
                Proxy[] partOfArray = new Proxy[xLength];
                Array.Copy(mPrxsArray, xStartFrom, partOfArray, 0, xLength);
                xStartFrom += xLength;

                var tpt = new TestProxies(ref mPrxsDic, mTargets[idTarget], partOfArray, mTimeOutTest);
                tpt.mTstDead += new EventHandler(TestProxiesThreadsListUpdate);
                tpt.mPrxsLstUpdated += new EventHandler(OnChanged);
                tpt.Start();

                mTestProxiesThreads.Add(tpt);

                idTarget = (idTarget + 1 > mTargets.Count-1) ? 0 : idTarget + 1;
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
        private void UpdateProxyLoadThreadsList(object sender, EventArgs e)
        {
            bool ok = false;
            while (!ok)
            {
                try
                {
                    foreach (ProxySearcher prsr in mProxyLoadThreads)
                    {
                        if (!prsr.IsRun())
                        {
                            mProxyLoadThreads.Remove(prsr);
                        }
                    }
                    mThreadsCountFind = mProxyLoadThreads.Count;
                    ok = true;
                }
                catch (Exception)
                {
                    ok = false;
                }
            }

            if (mThreadsCountFind == 0)
            {
                mIsRunFinding = false;
            }

            OnChanged(this, EventArgs.Empty);
        }

        private void TestProxiesThreadsListUpdate(object sender, EventArgs e)
        {
            bool ok = false;
            while (!ok)
            {
                try
                {
                    foreach (TestProxies tpt in mTestProxiesThreads)
                    {
                        if (!tpt.IsRun())
                        {
                            mTestProxiesThreads.Remove(tpt);
                        }
                    }
                    mThreadsCountTest = mTestProxiesThreads.Count;
                    ok = true;
                }
                catch (Exception)
                {
                    ok = false;
                }
            }

            if (mThreadsCountTest == 0)
            {
                mIsRunTesting = false;
            }

            OnChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// stop searching of proxies
        /// </summary>
        public void StopProxiesWorkers()
        {
            bool ok=false;
            if (mProxyLoadThreads != null)
            {
                while (!ok)
                {
                    try
                    {
                        foreach (ProxySearcher prsr in mProxyLoadThreads)
                        {
                            prsr.StopLoading();
                        }
                        ok = true;
                    }
                    catch (Exception)
                    {
                        ok =false;
                    }
                }
            }
            ok = false;
            if (mTestProxiesThreads != null)
            {
                while (!ok)
                {
                    try
                    {
                        foreach (TestProxies tpt in mTestProxiesThreads)
                        {
                            tpt.StopTesting();
                        }
                        ok = true;
                    }
                    catch (Exception)
                    {
                        ok = false;
                    }
                }
            }
            //OnChanged(this, EventArgs.Empty);
        }

        private void OnChanged(object sender, EventArgs e)
        {
            mPrxsArray = new Proxy[mPrxsDic.Count];

            int i = 0;
            bool ok = false;
            while (!ok)
            {
                try
                {
                    foreach (KeyValuePair<string, Proxy> p in mPrxsDic)
                    {
                        mPrxsArray[i] = p.Value;
                        i++;
                    }
                    ok = true;
                }
                catch (Exception)
                {
                    ok = false;
                }
            }

            mPrxsFound = (i>0)?true:false;

            if (mThreadsCountFind == 0)
            {
                mIsRunFinding = false;
                mProgressValue = 0;
            }
            if (mThreadsCountTest == 0)
            {
                mIsRunTesting = false;
                mProgressValue = 0;
            }

            if (!mIsRunFinding)
            {
                if(!mIsRunTesting)
                {
                    mStatus = String.Format("Done; found: {0}", i);
                }else
                {                    
                    mStatus = string.Format("Active threads: {0}; Found: {1}", mThreadsCountTest, i);
                }
            }
            else
            {
                if(!mIsRunTesting)
                {
                    i = (i >= mPrxsCountNeed) ? mPrxsCountNeed : i;
                    mProgressValue = Convert.ToInt32(Math.Round((double)i * 100 / mPrxsCountNeed, 0));
                    mStatus = string.Format("Active threads: {0}; Loaded: {1}", mThreadsCountFind, i);
                }                
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
