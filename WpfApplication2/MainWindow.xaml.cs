using System;
using System.Diagnostics;
using System.Media;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace prxSearcher
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ProxiesList pl;
        Settings mySettings;

        public MainWindow()
        {
            InitializeComponent();
            tbStatus.Text = "Idle";
            pbStatus.Visibility = Visibility.Collapsed;
            mySettings = new Settings();
            mySettings.LoadSettings();
        }

        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;

            MessageBox.Show(string.Format("ver: {0}", version),"About program",MessageBoxButton.OK,MessageBoxImage.Information,MessageBoxResult.OK);
        }

        private void menuExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void mainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Escape: 
                    if (pl != null)
                    {
                        if (pl.mIsRun)
                        {
                            pl.StopProxiesLoading();
                        }
                        else if (MessageBox.Show("Do you want exit?", "exit", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                        {
                            Close();
                        }
                    }
                    else
                    {
                        if (MessageBox.Show("Do you want exit?", "exit", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                        {
                            Close();
                        }
                    }
                    break;
                case Key.F1:
                    MenuAbout_Click(this, null);
                    break;
                case Key.F2:
                    menuSettings_Click(this, null);
                    break;
                case Key.F5:                    
                    menuFindPrxs_Click(this, null);
                    break;
                case Key.F6:
                    menuTestPrxs_Click(this, null);
                    break;
            }
        }        

        private void menuSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow f2 = new SettingsWindow() { DataContext = mySettings };
            f2.Owner = this;
            f2.Show();
        }

        private void menuFindPrxs_Click(object sender, RoutedEventArgs e)
        {
            if(pl != null)
            {
                if(pl.mIsRun)
                {
                    MessageBox.Show("Already running","Warning",MessageBoxButton.OK,MessageBoxImage.Warning);
                    return;
                }
            }
            string proxyParam = (mySettings.mUseProxy) ? mySettings.mProxy : String.Empty;

            pl = new ProxiesList(mySettings.mThreadsCount,
                                 mySettings.mNeedProxyCount,
                                 mySettings.mSearchers,
                                 proxyParam,
                                 mySettings.mSearchPhrase);
            
            dtUnsrtd.ItemsSource = pl;
            pbStatus.Visibility = Visibility.Visible;

            pl.Changed += new EventHandler(UpdateDataGrid);            
            pl.GetProxiesList();
        }

        private void mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(pl != null)
                pl.Dispose();
        }

        private void UpdateDataGrid(object sender, EventArgs e)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    CollectionViewSource.GetDefaultView(dtUnsrtd.ItemsSource).Refresh();
                    pbStatus.Value = pl.mProgressValue;
                    if (pbStatus.Value == 0 && !pl.mIsRun)
                    {
                        pbStatus.Visibility = Visibility.Collapsed;
                        string pathToWav = String.Format(@"{0}\Media\Windows Notify.wav", Environment.GetEnvironmentVariable("SystemRoot"));
                        using (var soundPlayer = new SoundPlayer(pathToWav))
                        {
                            soundPlayer.Play(); // can also use soundPlayer.PlaySync()
                        }
                    }
                    tbStatus.Text = pl.mStatus;
                });
            }
            catch (Exception) { }
        }

        private void menuTestPrxs_Click(object sender, RoutedEventArgs e)
        {
            if (pl != null)
                pl.TestProxiesDictionary(mySettings.mThreadsCount, "http://whatismyipaddress.com/");
        }
    }
}
