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
        private Settings s;

        public SettingsWindow()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(Page_Loaded);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(DataContext.ToString());
        }

        private void Page_Loaded(object sender, EventArgs e)
        {
            //s = DataContext as Settings;
            //ThreadsCount.DataContext = DataContext;
            //gSettings.DataContext = this.DataContext;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}
