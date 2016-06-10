using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var s = DataContext as Settings;
            s.SaveSettingsToFile();
            MessageBox.Show("Settings were saved");
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

        private void btnDef_Click(object sender, RoutedEventArgs e)
        {
            if(MessageBox.Show("Restore settings to defalut?","Question",MessageBoxButton.YesNo,MessageBoxImage.Question,MessageBoxResult.No)==MessageBoxResult.Yes)
            {
                var s = DataContext as Settings;
                s.RestoreSettingsToDefaults();
                s.LoadSettings();
                listBox.ItemsSource = null;
                listBox.ItemsSource = s.mSearchers;
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Owner.Focus();
        }
    }
}
