using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prxSearcher
{
    class Settings
    {
        //members
        public int mNeedProxyCount;
        public int mThreadsCount;
        public bool mUseProxy;
        public string mProxy;
        public Dictionary<string,Searcher> mSearchers;
        public string mDefaultSearcher;
        public string mPathToFileSettings;

        //methods
        public Settings()
        {
            if (!LoadSettings())
                throw new Exception("Can't load settings from file.");
        }

        public bool LoadSettings()
        {

            return true;
        }

        public bool SaveSettingsToFile()
        {

            return true;
        }
    }
}
