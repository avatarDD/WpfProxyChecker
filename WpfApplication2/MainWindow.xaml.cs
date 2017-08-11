using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace prxSearcher
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window,IDisposable
    {
        ProxiesList pl;
        Settings mySettings;

        public MainWindow()
        {
            InitializeComponent();
            tbStatus.Text = "Idle";
            pbStatus.Visibility = Visibility.Collapsed;
            mySettings = new Settings();
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
                        if (pl.mIsRunFinding || pl.mIsRunTesting)
                        {
                            pl.StopProxiesWorkers();
<<<<<<< HEAD
                            Thread.CurrentThread.IsBackground = true;
=======
>>>>>>> origin/feature
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
                case Key.F9:
                    menuSaveResultToFile_Click(this, null);
                    break;
            }            
        }        

        private void menuSaveResultToFile_Click(object sender, RoutedEventArgs e)
        {
            if (pl == null)
                return;
            if (!pl.mPrxsFound)
                return;
            SaveFileDialog fd = new SaveFileDialog();
            fd.FileName = mySettings.mPathToFileResult;
            fd.DefaultExt = ".txt";
            fd.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            fd.InitialDirectory = Directory.GetCurrentDirectory();
            if (fd.ShowDialog() == true)
            {
                pl.SaveResultToFile(fd.FileName);
                System.Diagnostics.Process.Start("explorer.exe", @"/select, " + fd.FileName);
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
                if(pl.mIsRunFinding)
                {
                    MessageBox.Show("Already finding is running","Warning",MessageBoxButton.OK,MessageBoxImage.Warning);
                    return;
                }
                else if(pl.mIsRunTesting)
                {
                    MessageBox.Show("Already testing is running", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            string proxyParam = (mySettings.mUseProxy) ? mySettings.mProxy : String.Empty;

            pl = new ProxiesList(mySettings.mFindThreadsCount,
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
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
                {
                    CollectionViewSource.GetDefaultView(dtUnsrtd.ItemsSource).Refresh();
                    menuTestPrxs.IsEnabled = pl.mPrxsFound;
                    menuSavePrxs.IsEnabled = pl.mPrxsFound;
                    pbStatus.Value = pl.mProgressValue;

                    if (pbStatus.Value == 0 && !pl.mIsRunFinding && !pl.mIsRunTesting)
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
            {
                if (!pl.mPrxsFound)
                {
                    MessageBox.Show("Find the proxies first", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                if(mySettings.mTargets.Count==0)
                {
                    MessageBox.Show("Fill list of targets first", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                pl.TestProxiesDictionary(mySettings.mTestThreadsCount,mySettings.mTargets);
                pbStatus.Visibility = Visibility.Collapsed;
            }
            else
            {
                MessageBox.Show("Find the proxies first", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public void Dispose()
        {
            pl.Dispose();
        }
    }
}
