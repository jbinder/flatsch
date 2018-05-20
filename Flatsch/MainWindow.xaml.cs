using System;
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
        private bool _isPaused = false;
        private double _lastOpacity = 0f;
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
            UpdateSettings();
        }

        private void UpdateSettings()
        {
            _fadeInAnimation = new DoubleAnimation
            {
                From = 0,
                To = Height,
                AutoReverse = false,
                Duration = new Duration(TimeSpan.FromMilliseconds(Settings.Default.FadeInAnimTime)),
            };
            Background = Settings.Default.IsTransparent ? Brushes.Transparent : new SolidColorBrush(Color.FromArgb(
                Settings.Default.BackgroundColor.A,
                Settings.Default.BackgroundColor.R,
                Settings.Default.BackgroundColor.G,
                Settings.Default.BackgroundColor.B));
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
            if (!Settings.Default.ShowFish) return;
            FishImage.BeginAnimation(HeightProperty, _fadeInAnimation);
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
            new SettingsWindow().ShowDialog();
            UpdateSettings();
        }
    }
}
