using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private const string TextModify = "Modify";
        private readonly Dictionary<string, Action> _defaultProfiles = new Dictionary<string, Action>
        {
            {"Constant Blink Reminder", SetConstantBlinkReminderDefaults},
            {"The 20-20-20 Rule", Set202020RuleDefaults},
            {"The 20-20-20 Rule (Dual Monitor, HD)", Set2020RuleDualMonitorHdDefaults}
        };
        private readonly Dictionary<string, Profile> _customProfiles = new Dictionary<string, Profile>();
        private Profile _currentProfile;
        private bool _hasChanges;
        private bool _profileHasChanges;
        private string _lastLoadedProfile;

        public SettingsWindow()
        {
            InitializeComponent();
            UpdateCustomProfiles();
            // the settings window is created each time the window is shown, so get the current profile here
            _currentProfile = GetProfileFromSettings();
            Closing += HandleOnClosing;
            EnableOnPropertyChangedListener();
            _lastLoadedProfile = Settings.Default.Profile;
        }

        private void EnableOnPropertyChangedListener()
        {
            Settings.Default.PropertyChanged += DefaultOnPropertyChanged;
        }

        private void DisableOnPropertyChangedListener()
        {
            Settings.Default.PropertyChanged -= DefaultOnPropertyChanged;
        }

        private void HandleOnClosing(object sender, CancelEventArgs e)
        {
            if (_hasChanges)
            {
                var result = MessageBox.Show("There are unsaved changes, really close?", "Warning", MessageBoxButton.YesNo);
                if (result != MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }
            DisableOnPropertyChangedListener();
            RevertSettings();
        }

        private void DefaultOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var newProfile = GetProfileFromSettings();
            if (XmlSerializerHelper.Serialize(newProfile) == XmlSerializerHelper.Serialize(_currentProfile))
            {
                _hasChanges = false;
                _profileHasChanges = false;
                UpdateProfileButtonContent();
                return;
            }
            if (_defaultProfiles.Keys.Contains(Profiles.Text))
            {
                Profiles.SelectedValue = string.Empty;
                _lastLoadedProfile = string.Empty;
                UpdateProfileButtonContent();
            }
            else if (!string.IsNullOrWhiteSpace(Profiles.Text) && _lastLoadedProfile == Profiles.Text)
            {
                ApplyProfile.Content = TextModify;
            }
            _hasChanges = true;
            _profileHasChanges = true;
        }

        private void Save_OnClick(object sender, RoutedEventArgs e)
        {
            DisableOnPropertyChangedListener();
            Settings.Default.Profile = _profileHasChanges ? string.Empty : _lastLoadedProfile; // revert unloaded profile selection
            Settings.Default.Save();
            _hasChanges = false;
            _currentProfile = GetProfileFromSettings();
            Close();
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void RevertSettings()
        {
            ApplyCustomProfile(_currentProfile);
            InitSettingProfiles();
        }

        private void Reset_OnClick(object sender, RoutedEventArgs e)
        {
            Settings.Default.Reset();
            InitSettingProfiles();
            _currentProfile = GetProfileFromSettings();
        }

        private void ApplyProfile_OnClick(object sender, RoutedEventArgs e)
        {
            DisableOnPropertyChangedListener();
            var action = (string)ApplyProfile.Content;
            if (action == TextAdd)
            {
                if (string.IsNullOrWhiteSpace(Profiles.Text))
                {
                    MessageBox.Show("Please specify a profile name!", "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                AddCurrentProfileToSettings();
                UpdateCustomProfiles();
                _lastLoadedProfile = Profiles.Text;
            }
            else if (action == TextModify)
            {
                var existingProfileXml = "";
                foreach (var profileXml in Settings.Default.Profiles)
                {
                    var profile = XmlSerializerHelper.Deserialize<Profile>(profileXml);
                    if (profile.Name == Profiles.Text)
                    {
                        existingProfileXml = profileXml;
                    }
                }
                Settings.Default.Profiles.Remove(existingProfileXml);
                _lastLoadedProfile = Profiles.Text;
                _profileHasChanges = false;
                AddCurrentProfileToSettings();
                UpdateCustomProfiles();
            }
            else
            {
                var profile = Profiles.Text;
                var hasBeenLoaded = false;

                if (_defaultProfiles.ContainsKey(Profiles.Text))
                {
                    _defaultProfiles[Profiles.Text]();
                    InitSettingProfiles();
                    hasBeenLoaded = true;
                }

                if (_customProfiles.ContainsKey(Profiles.Text))
                {
                    ApplyCustomProfile(_customProfiles[Profiles.Text]);
                    hasBeenLoaded = true;
                }

                if (hasBeenLoaded)
                {
                    _lastLoadedProfile = profile;
                    Settings.Default.Profile = profile;
                }
            }
            EnableOnPropertyChangedListener();
        }

        private void AddCurrentProfileToSettings()
        {
            var profile = GetProfileFromSettings();
            var serialized = XmlSerializerHelper.Serialize(profile);
            Settings.Default.Profiles.Add(serialized);
        }

        private void Profiles_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (Profiles.Text == string.Empty)
            {
                return;
            }
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
                if (_customProfiles.ContainsKey(profile.Name))
                {
                    _customProfiles.Remove(profile.Name);
                }
                _customProfiles.Add(profile.Name, profile);
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
