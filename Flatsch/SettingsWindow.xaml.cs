using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Flatsch.Helper;
using Flatsch.Models;

namespace Flatsch
{
    /// <summary>
    /// Interaction logic for WSettingsWindow.xaml
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public partial class SettingsWindow : Window
    {
        private const string TextLoad = "Load";
        private const string TextAdd = "Add";
        private readonly Dictionary<string, Action> _defaultProfiles = new Dictionary<string, Action>
        {
            {"Constant Blink Reminder", SetConstantBlinkReminderDefaults},
            {"The 20-20-20 Rule", Set202020RuleDefaults},
            {"The 20-20-20 Rule (Dual Monitor, HD)", Set2020RuleDualMonitorHdDefaults}
        };
        private readonly Dictionary<string, Profile> _customProfiles = new Dictionary<string, Profile>();

        public SettingsWindow()
        {
            InitializeComponent();
            UpdateCustomProfiles();
        }

        private void Save_OnClick(object sender, RoutedEventArgs e)
        {
            Settings.Default.Save();
            Close();
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            Settings.Default.Reload();
            InitSettingProfiles();
            Close();
        }

        private void Reset_OnClick(object sender, RoutedEventArgs e)
        {
            Settings.Default.Reset();
            InitSettingProfiles();
        }

        private void ApplyProfile_OnClick(object sender, RoutedEventArgs e)
        {
            if ((string)ApplyProfile.Content == TextAdd)
            {
                if (string.IsNullOrWhiteSpace(Profiles.Text))
                {
                    MessageBox.Show("Please specify a profile name!", "Profile name missing", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                var profile = GetProfileFromSettings();
                var serialized = XmlSerializerHelper.Serialize(profile);
                Settings.Default.Profiles.Add(serialized);

                UpdateCustomProfiles();
            }
            else
            {
                if (_defaultProfiles.ContainsKey(Profiles.Text))
                {
                    _defaultProfiles[Profiles.Text]();
                    InitSettingProfiles();
                }

                if (_customProfiles.ContainsKey(Profiles.Text))
                {
                    ApplyCustomProfile(_customProfiles[Profiles.Text]);
                }
            }
        }

        private void Profiles_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateProfileButtonContent();
        }

        private static void ApplyCustomProfile(Profile profile)
        {
            Settings.Default.ShowWindowTime = profile.ShowWindowTime;
            Settings.Default.HideWindowTime = profile.HideWindowTime;
            Settings.Default.NotificationText = profile.NotificationText;
            Settings.Default.SpanAcrossAllScreens = profile.SpanAcrossAllScreens;
            Settings.Default.FadeInAnimTime = profile.FadeInAnimTime;
            Settings.Default.FadeOutAnimTime = profile.FadeOutAnimTime;
            Settings.Default.IsSoundEnabled = profile.IsSoundEnabled;
            Settings.Default.BackgroundColor = profile.BackgroundColor;
            Settings.Default.ShowFish = profile.ShowFish;
            Settings.Default.Opacity = profile.Opacity;
            Settings.Default.NotificationTextMarginTop = profile.NotificationTextMarginTop;
            Settings.Default.NotificationTextFontSize = profile.NotificationTextFontSize;
            Settings.Default.Screen = profile.Screen;
        }

        private Profile GetProfileFromSettings()
        {
            var profile = new Profile
            {
                Name = Profiles.Text,
                ShowWindowTime = Settings.Default.ShowWindowTime,
                HideWindowTime = Settings.Default.HideWindowTime,
                NotificationText = Settings.Default.NotificationText,
                SpanAcrossAllScreens = Settings.Default.SpanAcrossAllScreens,
                FadeInAnimTime = Settings.Default.FadeInAnimTime,
                FadeOutAnimTime = Settings.Default.FadeOutAnimTime,
                IsSoundEnabled = Settings.Default.IsSoundEnabled,
                BackgroundColor = Settings.Default.BackgroundColor,
                ShowFish = Settings.Default.ShowFish,
                Opacity = Settings.Default.Opacity,
                NotificationTextMarginTop = Settings.Default.NotificationTextMarginTop,
                NotificationTextFontSize = Settings.Default.NotificationTextFontSize,
                Screen = Settings.Default.Screen
            };
            return profile;
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
            Settings.Default.Opacity = 0.25f;
            Settings.Default.ShowFish = false;
            Settings.Default.ShowWindowTime = 50;
            Settings.Default.FadeInAnimTime = 50;
            Settings.Default.FadeOutAnimTime = 50;
            Settings.Default.HideWindowTime = 5000;
            Settings.Default.NotificationText = "";
            Settings.Default.SpanAcrossAllScreens = true;
        }
        
        private void UpdateCustomProfiles()
        {
            foreach (var profileXml in Settings.Default.Profiles)
            {
                var profile = XmlSerializerHelper.Deserialize<Profile>(profileXml);
                if (!_customProfiles.ContainsKey(profile.Name))
                {
                    _customProfiles.Add(profile.Name, profile);
                }
            }

            Profiles.ItemsSource = _defaultProfiles.Keys.Concat(_customProfiles.Keys);
            UpdateProfileButtonContent();
        }

        private void InitSettingProfiles()
        {
            Settings.Default.Profiles.Clear();
            Settings.Default.Profiles.AddRange(_customProfiles.Values.Select(XmlSerializerHelper.Serialize).ToArray());
        }

        private void UpdateProfileButtonContent()
        {
            var text = Profiles.Text;
            if (_defaultProfiles.ContainsKey(text) || _customProfiles.ContainsKey(text))
            {
                ApplyProfile.Content = TextLoad;
            }
            else
            {
                ApplyProfile.Content = TextAdd;
            }
        }
    }
}
