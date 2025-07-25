using GTweak.Utilities.Animation;
using GTweak.Utilities.Controls;
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace GTweak.Windows
{
    public partial class MessageWindow
    {
        private readonly DispatcherTimer _timer;
        private TimeSpan _time = TimeSpan.FromSeconds(4);

        public MessageWindow(bool isViolationSystem = false)
        {
            InitializeComponent();

            try
            {
                _timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
                {
                    BtnAccept.Content = $"{new Regex("[(05)(04)(03)(02)]").Replace(BtnAccept.Content.ToString(), "")}({_time:ss})";
                    if (_time == TimeSpan.Zero) { _timer?.Stop(); Application.Current.Shutdown(); }
                    _time = _time.Add(TimeSpan.FromSeconds(-1));
                }, Application.Current.Dispatcher);

            }
            catch (Exception ex) { ErrorLogging.LogDebug(ex); }
            _timer?.Start();

            if (isViolationSystem)
            {
                NoSupportWarn.Visibility = Visibility.Visible;
                CreateWarn.Visibility = Visibility.Hidden;
            }
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void BtnAccept_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                Application.Current.Shutdown();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Closing -= Window_Closing;
            e.Cancel = true;
            BeginAnimation(OpacityProperty, FadeAnimation.FadeTo(0.1, () => { Close(); }));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) => BeginAnimation(OpacityProperty, FadeAnimation.FadeIn(1, 0.2));
    }
}
