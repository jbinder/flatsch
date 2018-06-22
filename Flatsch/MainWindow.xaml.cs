﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Media;
using System.Timers;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Flatsch.Helper;
using Application = System.Windows.Application;
using Color = System.Windows.Media.Color;
using MenuItem = System.Windows.Controls.MenuItem;
using Rectangle = System.Drawing.Rectangle;
using Timer = System.Timers.Timer;

namespace Flatsch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _isPaused = false;
        private double _lastOpacity = 0f;
        private SoundPlayer _player;
        private DoubleAnimation _fishAnimation;
        private DoubleAnimation _textAnimation;
        private const int WM_DEVICECHANGE = 0x219;
        private const int WM_DISPLAYCHANGE = 0x7e;

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
            Opacity = Settings.Default.Opacity;
            Properties.Settings.Default.PropertyChanged += PropertyChanged;
            UpdateSettings();
        }

        private void PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // UpdateSettings();
        }

        private void UpdateSettings()
        {
            _fishAnimation = new DoubleAnimation
            {
                From = 0,
                To = Height,
                AutoReverse = false,
                Duration = new Duration(TimeSpan.FromMilliseconds(Settings.Default.FadeInAnimTime)),
            };
            _textAnimation = new DoubleAnimation
            {
                From = 0f,
                To = 1f,
                AutoReverse = false,
                Duration = new Duration(TimeSpan.FromMilliseconds(Settings.Default.FadeInAnimTime)),
            };
            Background = new SolidColorBrush(Color.FromArgb(
                Settings.Default.BackgroundColor.A,
                Settings.Default.BackgroundColor.R,
                Settings.Default.BackgroundColor.G,
                Settings.Default.BackgroundColor.B));
            SetWindowPosAndSize();
            UpdateWindowContent();
        }

        private void UpdateWindowContent()
        {
            FishImage.Visibility = Settings.Default.ShowFish ? Visibility.Visible : Visibility.Collapsed;
            BlinkReminderText.Visibility = Settings.Default.ShowFish ? Visibility.Collapsed : Visibility.Visible;
            var customFontSize = Settings.Default.NotificationTextFontSize > 0;
            Viewbox.Stretch = Settings.Default.ShowFish || customFontSize ? Stretch.None : Stretch.Uniform;
            if (customFontSize)
            {
                BlinkReminderText.Margin = new Thickness(0, Settings.Default.NotificationTextMarginTop, 0, 0);
                BlinkReminderText.FontSize = Settings.Default.NotificationTextFontSize;
            }
        }

        private void SetWindowPosAndSize()
        {
            if (!Settings.Default.SpanAcrossAllScreens) {
                var screen = (Settings.Default.Screen >= 0 && Settings.Default.Screen < Screen.AllScreens.Length)
                    ? Screen.AllScreens[Settings.Default.Screen]
                    : Screen.PrimaryScreen;
                Width = screen.WorkingArea.Width;
                Height = screen.WorkingArea.Height;
                Top = screen.WorkingArea.Top;
                Left = screen.WorkingArea.Left;
            }
            else
            {
                var area = Screen.AllScreens.Aggregate(new Rectangle(), (x, y) => Rectangle.Union(x, y.WorkingArea));
                Top = area.Top;
                Left = area.Left;
                Width = area.Width;
                Height = area.Height;
            }
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

        private void Start()
        {
            Opacity = _lastOpacity;
            _timer.Enabled = true;
        }

        private void Stop()
        {
            _lastOpacity = Opacity;
            Opacity = 0f;
            _timer.Enabled = false;
        }

        private void SetHideWindowTimer()
        {
            _timer.Elapsed -= OnShowWindow;
            _timer.Elapsed += OnHideWindow;
            _timer.Interval = Settings.Default.ShowWindowTime;
        }

        private void SetShowWindowTimer()
        {
            _timer.Elapsed -= OnHideWindow;
            _timer.Elapsed += OnShowWindow;
            _timer.Interval = Settings.Default.HideWindowTime;
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
            if (!Settings.Default.ShowFish)
            {
                BlinkReminderText.BeginAnimation(OpacityProperty, _textAnimation);
            }
            else
            {
                FishImage.BeginAnimation(HeightProperty, _fishAnimation);
            }
        }

        private void PlaySound()
        {
            if (!Settings.Default.IsSoundEnabled) return;
            _player.Play();
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
            // Allow clicking through the window, hide from program switcher
            var hwnd = new WindowInteropHelper(this).Handle;
            WindowHelper.PrepareWindow(hwnd);

            // Detect screen changes and move window to an active screen
            var source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_DEVICECHANGE || msg == WM_DISPLAYCHANGE)
            {
                SetWindowPosAndSize();
            }
            return IntPtr.Zero;
        }

        private void MenuShowFish_OnClick(object sender, RoutedEventArgs e)
        {
            var item = (MenuItem) sender;
            item.IsChecked = !item.IsChecked;
            SaveSettings();
            UpdateWindowContent();
        }

        private void MenuItemPause_OnClick(object sender, RoutedEventArgs e)
        {
            var item = (MenuItem) sender;
            _isPaused = !_isPaused;
            if (!_isPaused)
            {
                Start();
            }
            else
            {
                Stop();
            }
            item.IsChecked = _isPaused;
        }

        private void MenuSettings_OnClick(object sender, RoutedEventArgs e)
        {
            var item = (MenuItem) sender;
            item.IsEnabled = false;
            new SettingsWindow{Owner = this}.ShowDialog();
            item.IsEnabled = true;
            UpdateSettings();
        }
    }
}
