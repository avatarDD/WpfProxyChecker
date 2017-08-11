using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace prxSearcher
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            ThreadsCount.Focus();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var s = DataContext as Settings;
            s.SaveSettingsToFile();
            MessageBox.Show("Settings were saved","Info",MessageBoxButton.OK,MessageBoxImage.Information,MessageBoxResult.OK);
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var s = DataContext as Settings;
            var srchr = new Searcher();
            srchr.first = 0;
            srchr.pageVar = "";
            srchr.regexExpOfResults = "";
            srchr.spltr = "";
            srchr.srchVar = "";
            srchr.step = 0;
            srchr.url = "";
            listBox.ItemsSource = null;
            s.mSearchers.Add(srchr);
            listBox.ItemsSource = s.mSearchers;
            listBox.SelectedIndex = listBox.Items.Count - 1;
            url.Focus();
        }

        private void btnAddTarget_Click(object sender, RoutedEventArgs e)
        {
            var s = DataContext as Settings;
            var target = new Target();
            target.mAdress = "";
            target.mRegexContry = "";
            listBoxTargets.ItemsSource = null;
            s.mTargets.Add(target);
            listBoxTargets.ItemsSource = s.mTargets;
            listBoxTargets.SelectedIndex = listBoxTargets.Items.Count - 1;
            tbTargetAdress.Focus();
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            var s = DataContext as Settings;
            var selectedElmnt = listBox.SelectedItem as Searcher;

            listBox.SelectedIndex = -1;
            listBox.ItemsSource = null;
            s.mSearchers.Remove(selectedElmnt);

            listBox.ItemsSource = s.mSearchers;
            listBox.SelectedIndex = listBox.Items.Count - 1;
            url.Focus();
        }

        private void btnDelTarget_Click(object sender, RoutedEventArgs e)
        {
            var s = DataContext as Settings;
            var selectedElmnt = listBoxTargets.SelectedItem as Target;

            listBoxTargets.SelectedIndex = -1;
            listBoxTargets.ItemsSource = null;
            s.mTargets.Remove(selectedElmnt);

            listBoxTargets.ItemsSource = s.mTargets;
            listBoxTargets.SelectedIndex = listBoxTargets.Items.Count - 1;
            tbTargetAdress.Focus();
        }

        private void btnDef_Click(object sender, RoutedEventArgs e)
        {
            if(MessageBox.Show("Restore settings to defalut?","Question",MessageBoxButton.YesNo,MessageBoxImage.Question,MessageBoxResult.No)==MessageBoxResult.Yes)
            {
                var s = DataContext as Settings;
                s.RestoreSettingsToDefaults();
                s.LoadSettings();

                listBox.ItemsSource = null;
                listBox.ItemsSource = s.mSearchers;

                listBoxTargets.ItemsSource = null;
                listBoxTargets.ItemsSource = s.mTargets;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var s = DataContext as Settings;
            var lb = sender as ListBox;
            if (lb.SelectedIndex > -1)
            {
                sP.DataContext = s.mSearchers[lb.SelectedIndex];                
            }
        }

        private void listBoxTargets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var s = DataContext as Settings;
            var lb = sender as ListBox;
            if (lb.SelectedIndex > -1)
            {
                sP1.DataContext = s.mTargets[lb.SelectedIndex];
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Owner.Focus();
        }

        private void SelectResultFilePath_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog fd = new SaveFileDialog();
            fd.FileName = PathToFileResult.Text;
            fd.DefaultExt = ".txt";
            fd.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            fd.InitialDirectory = Directory.GetCurrentDirectory();
            if (fd.ShowDialog() == true)
            {
                PathToFileResult.Text = fd.SafeFileName;
            }
        }
    }
}
