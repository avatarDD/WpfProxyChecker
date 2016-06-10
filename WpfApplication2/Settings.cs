using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace prxSearcher
{
    [DataContract]
    class Settings
    {
        //members
        [DataMember]
        public int mNeedProxyCount { get; set; }
        [DataMember]
        public int mThreadsCount { get; set; }
        [DataMember]
        public bool mUseProxy { get; set; }
        [DataMember]
        public string mProxy { get; set; }
        [DataMember]
        public string mSearchPhrase { get; set; }
        [DataMember]
        public List<Searcher> mSearchers { get; set; }
        [DataMember]
        public string mPathToFileSettings { get; set; }
        [DataMember]
        public string mPathToFileResult { get; set; }

        //methods
        public Settings()
        {
            mPathToFileSettings = "Settings.json";            
        }       

        public void LoadSettings()
        {
            try
            {
                DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(Settings));
                using (FileStream fs = new FileStream(mPathToFileSettings, FileMode.Open))
                {
                    Settings newSettings = (Settings)jsonFormatter.ReadObject(fs);
                    mNeedProxyCount = newSettings.mNeedProxyCount;
                    mThreadsCount = newSettings.mThreadsCount;
                    mUseProxy = newSettings.mUseProxy;
                    mProxy = newSettings.mProxy;
                    mSearchPhrase = newSettings.mSearchPhrase;
                    mPathToFileSettings = newSettings.mPathToFileSettings;
                    mPathToFileResult = newSettings.mPathToFileResult;
                    mSearchers = newSettings.mSearchers;
                }
            }
            catch (Exception)
            {
                RestoreSettingsToDefaults();
                SaveSettingsToFile();
                LoadSettings();
                throw new Exception("Can't read settings from file and settings was restored to default");
            }
        }

        public void SaveSettingsToFile()
        {
            try
            {
                DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(Settings));
                using (FileStream fs = new FileStream(mPathToFileSettings, FileMode.OpenOrCreate))
                {
                    jsonFormatter.WriteObject(fs, this);
                }
            }
            catch(Exception ex)
            {
                throw new Exception(String.Format("Can't write settings to file.\r\n{0}",ex.Message));
            }
        }

        public void RestoreSettingsToDefaults()
        {
            mNeedProxyCount = 500;
            mThreadsCount = 20;
            mUseProxy = false;
            mProxy = "127.0.0.1:3128";
            mSearchPhrase = "proxy list";
            mPathToFileResult = "ProxiesList.txt";
            mPathToFileSettings = "Settings.json";

            mSearchers = new List<Searcher>();
            
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
                url = "http://search.yahoo.com/search?",
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
    }
}
