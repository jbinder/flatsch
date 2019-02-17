using System;
using System.Collections.Generic;
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
    // ReSharper disable once RedundantExtendsListEntry
    public partial class MainWindow : Window
    {
        private bool _isPaused;
        private bool _isFadingOut = true;
        private SoundPlayer _player;
        private readonly Dictionary<string, DoubleAnimation> _animations = new Dictionary<string, DoubleAnimation>();
        // ReSharper disable once InconsistentNaming
        private const int WM_DEVICECHANGE = 0x219;
        // ReSharper disable once InconsistentNaming
        private const int WM_DISPLAYCHANGE = 0x7e;
        private const int BackgroundAnimInterval = 20;

        private readonly Timer _timer = new Timer();
        private readonly Timer _backgroundAnimTimer = new Timer();
        private IntPtr hwnd;
        private DateTime _backgroundAnimStartTime;

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
            // Properties.Settings.Default.PropertyChanged += PropertyChanged;
            UpdateSettings();
            _backgroundAnimTimer.Interval = BackgroundAnimInterval;
            _backgroundAnimTimer.Elapsed -= OnBackgroundAnimTimer;
            _backgroundAnimTimer.Elapsed += OnBackgroundAnimTimer;
        }

        private void PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // UpdateSettings();
        }

        private void UpdateSettings()
        {
            _animations["fish_in"] = new DoubleAnimation
            {
                From = 0,
                To = Height,
                AutoReverse = false,
                Duration = new Duration(TimeSpan.FromMilliseconds(Settings.Default.FadeInAnimTime)),
            };
            _animations["text_in"] = new DoubleAnimation
            {
                From = 0f,
                To = 1f,
                AutoReverse = false,
                Duration = new Duration(TimeSpan.FromMilliseconds(Settings.Default.FadeInAnimTime)),
            };
            _animations["fish_out"] = new DoubleAnimation
            {
                From = Height,
                To = 0,
                AutoReverse = false,
                Duration = new Duration(TimeSpan.FromMilliseconds(Settings.Default.FadeOutAnimTime))
            };
            _animations["text_out"] = new DoubleAnimation
            {
                From = 1f,
                To = 0f,
                AutoReverse = false,
                Duration = new Duration(TimeSpan.FromMilliseconds(Settings.Default.FadeOutAnimTime)),
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
            Start();
        }

        private void Start()
        {
            Dispatcher.Invoke(() =>
            {
                Opacity = 0f;
                SetShowWindowTimer();
                _timer.Start();
            });
        }

        private void Stop()
        {
            Dispatcher.Invoke(() =>
            {
                Opacity = 0f;
                _timer.Stop();
                _backgroundAnimTimer.Stop();
            });
        }

        private void SetHideWindowTimer()
        {
            ResetTimer();
            _timer.Elapsed += OnHideWindow;
            _timer.Interval = Settings.Default.ShowWindowTime;
            _timer.Start();
        }

        private void SetShowWindowTimer()
        {
            ResetTimer();
            _timer.Elapsed += OnShowWindow;
            _timer.Interval = Settings.Default.HideWindowTime;
            _timer.Start();
        }

        private void SetShowWindowAnimDoneTimer()
        {
            ResetTimer();
            _timer.Elapsed += OnShowWindowAnimDone;
            _timer.Interval = Settings.Default.FadeInAnimTime;
            _timer.Start();
        }

        private void SetHideWindowAnimDoneTimer()
        {
            ResetTimer();
            _timer.Elapsed += OnHideWindowAnimDone;
            _timer.Interval = Settings.Default.FadeOutAnimTime;
            _timer.Start();
        }

        private void ResetTimer()
        {
            _timer.Stop();
            _timer.Elapsed -= OnHideWindow;
            _timer.Elapsed -= OnHideWindowAnimDone;
            _timer.Elapsed -= OnShowWindow;
            _timer.Elapsed -= OnShowWindowAnimDone;
        }

        private void OnHideWindow(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                PlayAnimation(false);
                SetHideWindowAnimDoneTimer();
            });
        }

        private void OnShowWindow(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                PlayAnimation(true);
                PlaySound();
                // in some situations the window gets pushed back behind other windows, so move set as topmost each loop
                WindowHelper.PrepareWindow(hwnd);
                SetShowWindowAnimDoneTimer();
            });
        }

        private void OnShowWindowAnimDone(object sender, ElapsedEventArgs e)
        {
            SetHideWindowTimer();
        }

        private void OnHideWindowAnimDone(object sender, ElapsedEventArgs e)
        {
            if (_isPaused)
            {
                // when paused, stop after a preview
                Stop();
            }
            else
            {
                SetShowWindowTimer();
            }
        }

        private void PlayAnimation(bool playInAnim)
        {
            var animType = playInAnim ? "in" : "out";
            if (!Settings.Default.ShowFish)
            {
                BlinkReminderText.BeginAnimation(OpacityProperty, _animations[$"text_{animType}"]);
            }
            else
            {
                FishImage.BeginAnimation(HeightProperty, _animations[$"fish_{animType}"]);
            }
            _isFadingOut = !playInAnim;
            if (playInAnim)
            {
                Opacity = 0f;
                _backgroundAnimStartTime = DateTime.UtcNow;
                _backgroundAnimTimer.Start();
            }
            else
            {
                Opacity = Settings.Default.Opacity;
                _backgroundAnimStartTime = DateTime.UtcNow;
                _backgroundAnimTimer.Start();
            }

        }

        private void OnBackgroundAnimTimer(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                var elapsedTime = (DateTime.UtcNow - _backgroundAnimStartTime).TotalMilliseconds;
                var currentAnimPosition = (float) elapsedTime / (_isFadingOut ? Settings.Default.FadeOutAnimTime : Settings.Default.FadeInAnimTime);
                if (currentAnimPosition > 1f)
                {
                    _backgroundAnimTimer.Stop();
                }
                Opacity = Settings.Default.Opacity * (_isFadingOut ? 1 - currentAnimPosition : currentAnimPosition);
                if ((Opacity <= 0 && _isFadingOut) || (Opacity >= Settings.Default.Opacity && !_isFadingOut))
                {
                    _backgroundAnimTimer.Stop();
                }
            });
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

        private void MainWindow_OnSourceInitialized(object sender, EventArgs e)
        {
            // Allow clicking through the window, hide from program switcher
            hwnd = new WindowInteropHelper(this).Handle;
            WindowHelper.PrepareWindow(hwnd);

            // Detect screen changes and move window to an active screen
            var source = PresentationSource.FromVisual(this) as HwndSource;
            source?.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_DEVICECHANGE || msg == WM_DISPLAYCHANGE)
            {
                SetWindowPosAndSize();
            }
            return IntPtr.Zero;
        }

        private void MenuItemPreview_OnClick(object sender, RoutedEventArgs e)
        {
            StartPreview();
        }

        private void StartPreview()
        {
            Start();
            _timer.Interval = 1; // immediately show
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
            if (!_isPaused)
            {
                Stop();
            }
            new SettingsWindow{Owner = this}.ShowDialog();
            item.IsEnabled = true;
            UpdateSettings();
            if (!_isPaused)
            {
                Start();
            }
        }

        private void TaskbarIcon_OnTrayLeftMouseUp(object sender, RoutedEventArgs e)
        {
            StartPreview();
        }
    }
}
