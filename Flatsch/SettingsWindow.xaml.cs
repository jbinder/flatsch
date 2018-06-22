using System.Windows;
using System.Windows.Media;

namespace Flatsch
{
    /// <summary>
    /// Interaction logic for WSettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void Save_OnClick(object sender, RoutedEventArgs e)
        {
            Settings.Default.Save();
            Close();
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            Settings.Default.Reload();
            Close();
        }

        private void Reset_OnClick(object sender, RoutedEventArgs e)
        {
            Settings.Default.Reset();
        }
    }
}
