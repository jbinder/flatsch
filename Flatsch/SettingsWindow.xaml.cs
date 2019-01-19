using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Flatsch
{
    /// <summary>
    /// Interaction logic for WSettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly Dictionary<string, Action> _defaultProfiles = new Dictionary<string, Action>
        {
            {"Constant Blink Reminder", SetConstantBlinkReminderDefaults},
            {"The 20-20-20 Rule", Set202020RuleDefaults},
            {"The 20-20-20 Rule (Dual Monitor, HD)", Set2020RuleDualMonitorHdDefaults}
        };

        public SettingsWindow()
        {
            InitializeComponent();
            Profiles.ItemsSource = _defaultProfiles.Keys;
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

        private void ApplyProfile_OnClick(object sender, RoutedEventArgs e)
        {
            if (_defaultProfiles.ContainsKey(Profiles.Text))
            {
                _defaultProfiles[Profiles.Text]();
            }

            Settings.Default.SelectedProfile = "";
        }

        private static void Set2020RuleDualMonitorHdDefaults()
        {
            Set202020RuleDefaults();
            Settings.Default.NotificationText = "👀   👀";
            Settings.Default.SpanAcrossAllScreens = true;
            Settings.Default.NotificationTextFontSize = 1000;
            Settings.Default.NotificationTextMarginTop = -250;
        }

        private static void Set202020RuleDefaults()
        {
            Settings.Default.Reset();
            Settings.Default.BackgroundColor = Colors.Transparent;
            Settings.Default.IsSoundEnabled = false;
            Settings.Default.Opacity = 1f;
            Settings.Default.ShowFish = false;
            Settings.Default.NotificationText = "👀";
            Settings.Default.SpanAcrossAllScreens = false;
            Settings.Default.FadeInAnimTime = 10000;
            Settings.Default.FadeOutAnimTime = 10000;
            Settings.Default.ShowWindowTime = 10000;
            Settings.Default.HideWindowTime = 20 * 60 * 1000;
        }

        private static void SetConstantBlinkReminderDefaults()
        {
            Settings.Default.Reset();
            Settings.Default.BackgroundColor = Colors.Black;
            Settings.Default.IsSoundEnabled = false;
            Settings.Default.Opacity = 0.5f;
            Settings.Default.ShowFish = false;
            Settings.Default.ShowWindowTime = 50;
            Settings.Default.FadeInAnimTime = 10;
            Settings.Default.FadeOutAnimTime = 10;
            Settings.Default.HideWindowTime = 5000;
            Settings.Default.NotificationText = "";
            Settings.Default.SpanAcrossAllScreens = true;
        }
    }
}
