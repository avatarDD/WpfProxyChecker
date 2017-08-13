using System;
using System.Collections.Generic;
using System.Threading;

namespace prxSearcher
{
    class TestProxies
    {
        private Thread mT;
        private bool mIsRun { get; set; }
        private Dictionary<string, Proxy> mPrxsDic;
        private Proxy[] mArray;
        private Target mTarget;

        public event EventHandler mTstDead;
        public event EventHandler mPrxsLstUpdated;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="PrxsDic">Dictionary of all proxies</param>
        /// <param name="Target">Site for testing proxies</param>
        /// <param name="xArray">part of proxies for testing in this thread</param>
        public TestProxies(ref Dictionary<string, Proxy> PrxsDic, Target Trgt, Proxy[] xArray)
        {
            mPrxsDic = PrxsDic;
            mArray = xArray;
            mTarget = Trgt;

            mT = new Thread(new ThreadStart(Test));
            mIsRun = true;
            mT.IsBackground = true;
            mT.Priority = ThreadPriority.Lowest;
        }

        private void Test()
        {
            for (int i = 0; i < mArray.Length && mIsRun; i++)
            {
                string html;
                double t;                               
                if(Web_Client.Get(mTarget.mAdress, mArray[i].adress, out html, out t))
                {
                    FillProperties(mArray[i], html, t, "http");
                }
                else if(Web_Client.GetViaSocks(true, mTarget.mAdress,mArray[i].adress, out html, out t))
                {
                    FillProperties(mArray[i], html, t, "socks5");
                }
                else if (Web_Client.GetViaSocks(false, mTarget.mAdress, mArray[i].adress, out html, out t))
                {
                    FillProperties(mArray[i], html, t, "socks4");
                }
                else
                {
                    lock(mPrxsDic)
                    {
                        mPrxsDic.Remove(mArray[i].adress);
                        mPrxsLstUpdated(this, EventArgs.Empty);
                    }
                }
            }
            mIsRun = false;
            mTstDead(this, EventArgs.Empty);
        }

        public bool IsRun()
        {
            return mIsRun;
        }

        public void Start()
        {
            mT.Start();
        }

        private void FillProperties(Proxy prx, string html, double t, string proxyType)
        {
            string country = GetCountry(html, mTarget.mRegexContry);
            string type_p = proxyType;
            lock (mPrxsDic)
            {
                mPrxsDic[prx.adress].latency = Math.Round(t, 0);
                mPrxsDic[prx.adress].country = country;
                mPrxsDic[prx.adress].type = type_p;
                mPrxsLstUpdated(this, EventArgs.Empty);
            }
        }

        private string GetCountry(string html, string regex)
        {
            return html_parser.Match(html, regex);
        }

        public void StopTesting()
        {
            mIsRun = false;
        }
    }
}
