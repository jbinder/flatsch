﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Flatsch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int ShowWindowTime = 2000;
        private const int FadeInAnimTime = 100;
        private const int HideWindowTime = 3000;

        private Brush _lastBackground;
        private bool _isSoundEnabled = true;
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
                FishImage.BeginAnimation(HeightProperty, _fadeInAnimation);
                PlaySound();
                Opacity = 1f;
            });
            SetHideWindowTimer();
        }

        private void PlaySound()
        {
            if (!_isSoundEnabled) return;
            _player.Play();
        }

        private void MenuItemTransparent_OnClick(object sender, RoutedEventArgs e)
        {
            var item = (MenuItem) sender;
            if (!item.IsChecked)
            {
                _lastBackground = Background;
            }
            Dispatcher.Invoke(() => { Background = item.IsChecked ? _lastBackground : Brushes.Transparent; });
            item.IsChecked = !item.IsChecked;
        }

        private void MenuItemQuit_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuItemEnableSound_OnClick(object sender, RoutedEventArgs e)
        {
            var item = (MenuItem) sender;
            item.IsChecked = !item.IsChecked;
            _isSoundEnabled = item.IsChecked;
        }
    }
}
