﻿using System;
using System.Media;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Flatsch.Helper;

namespace Flatsch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int ShowWindowTime = 200;
        private const int FadeInAnimTime = 100;
        private const int HideWindowTime = 3000;

        private SoundPlayer _player;
        private DoubleAnimation _fadeInAnimation;

        private readonly Timer _timer = new Timer();

        public MainWindow()
        {
            InitializeComponent();
            SetWindowPosAndSize();
            Initialize();
        }

        private void Initialize()
        {
            _player = new SoundPlayer("res/fish.wav");
            _fadeInAnimation = new DoubleAnimation
            {
                From = 0,
                To = Height,
                AutoReverse = false,
                Duration = new Duration(TimeSpan.FromMilliseconds(FadeInAnimTime)),
            };
            Background = Settings.Default.IsTransparent ? Brushes.Transparent : Settings.Default.BackgroundColor;
            UpdateShowFishSetting();
        }

        private void UpdateShowFishSetting()
        {
            FishImage.Visibility = Settings.Default.ShowFish ? Visibility.Visible : Visibility.Collapsed;
            BlinkReminderText.Visibility = Settings.Default.ShowFish ? Visibility.Collapsed : Visibility.Visible;
            Viewbox.Stretch = Settings.Default.ShowFish ? Stretch.None : Stretch.Uniform;
        }

        private void SetWindowPosAndSize()
        {
            Width = SystemParameters.WorkArea.Width;
            Height = SystemParameters.WorkArea.Height;
            Top = 0f;
            Left = 0f;
        }

        /// <summary>
        /// Try forcing the window to be always on top.
        /// </summary>
        private void MainWindow_OnDeactivated(object sender, EventArgs e)
        {
            var window = (Window)sender;
            window.Topmost = true;
        }

        /// <summary>
        /// Start the timer to show the message in a specified interval.
        /// </summary>
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            SetHideWindowTimer();
            _timer.Enabled = true;
        }

        private void SetHideWindowTimer()
        {
            _timer.Elapsed -= OnShowWindow;
            _timer.Elapsed += OnHideWindow;
            _timer.Interval = ShowWindowTime;
        }

        private void SetShowWindowTimer()
        {
            _timer.Elapsed -= OnHideWindow;
            _timer.Elapsed += OnShowWindow;
            _timer.Interval = HideWindowTime;
        }

        private void OnHideWindow(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() => { Opacity = 0f; });
            SetShowWindowTimer();
        }

        private void OnShowWindow(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                PlayAnimation();
                PlaySound();
                Opacity = Settings.Default.Opacity;
            });
            SetHideWindowTimer();
        }

        private void PlayAnimation()
        {
            if (!Settings.Default.ShowFish) return;
            FishImage.BeginAnimation(HeightProperty, _fadeInAnimation);
        }

        private void PlaySound()
        {
            if (!Settings.Default.IsSoundEnabled) return;
            _player.Play();
        }

        private void MenuItemTransparent_OnClick(object sender, RoutedEventArgs e)
        {
            var item = (MenuItem) sender;
            item.IsChecked = !item.IsChecked;
            Dispatcher.Invoke(() => { Background = !item.IsChecked ? Settings.Default.BackgroundColor : Brushes.Transparent; });
            SaveSettings();
        }

        private static void SaveSettings()
        {
            Settings.Default.Save();
        }

        private void MenuItemQuit_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuItemEnableSound_OnClick(object sender, RoutedEventArgs e)
        {
            var item = (MenuItem) sender;
            item.IsChecked = !item.IsChecked;
            SaveSettings();
        }

        private void MainWindow_OnSourceInitialized(object sender, EventArgs e)
        {
            // Allow clicking through the window
            var hwnd = new WindowInteropHelper(this).Handle;
            WindowHelper.EnableClickThrough(hwnd);
        }

        private void MenuShowFish_OnClick(object sender, RoutedEventArgs e)
        {
            var item = (MenuItem) sender;
            item.IsChecked = !item.IsChecked;
            SaveSettings();
            UpdateShowFishSetting();
        }
    }
}
