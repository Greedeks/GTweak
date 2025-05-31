using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Helpers.Animation;
using System;
using System.ComponentModel;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace GTweak.Windows
{
    public partial class NotificationWindow
    {
        private readonly DispatcherTimer timer;
        private TimeSpan time = TimeSpan.FromSeconds(3);

        private string clickAction = string.Empty;
        internal string TitleNotice { set => TitleSet.Text = value; get => TitleSet.Text; }
        internal string TextNotice { set => TextSet.Text = value; get => TextSet.Text; }
        internal string ActionNotice { set => clickAction = value; get => clickAction; }

        public NotificationWindow()
        {
            InitializeComponent();

            timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                if (time == TimeSpan.Zero) { timer.Stop(); Close(); }
                time = time.Add(TimeSpan.FromSeconds(-1));
            }, Application.Current.Dispatcher);

            timer.Start();
        }

        private void BtnExit_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                Close();
        }

        private void ActionSelection_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !string.IsNullOrEmpty(ActionNotice))
            {
                CommandExecutor.RunCommand(ActionNotice switch
                {
                    "logout" => @"/c logoff",
                    _ => @"/c shutdown /r /t 0",
                });
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (SettingsRepository.IsPlayingSound)
            {
                using SoundPlayer notificationSound = new SoundPlayer(Properties.Resources.Sound);
                notificationSound.Play();
            }

            Dispatcher.Invoke(() =>
            {
                Rect primaryMonitorArea = SystemParameters.WorkArea;

                Top = primaryMonitorArea.Bottom - Height - 10;
                Left = primaryMonitorArea.Right - Width - 10;

                DoubleAnimationUsingKeyFrames doubleAnimKeyFrames = new DoubleAnimationUsingKeyFrames();

                EasingDoubleKeyFrame fromFrame = new EasingDoubleKeyFrame(primaryMonitorArea.Right)
                {
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0)),
                    EasingFunction = new QuadraticEase()
                };

                EasingDoubleKeyFrame toFrame = new EasingDoubleKeyFrame(primaryMonitorArea.Right - Width - 10)
                {
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(200)),
                    EasingFunction = new QuadraticEase()
                };

                doubleAnimKeyFrames.KeyFrames.Add(fromFrame);
                doubleAnimKeyFrames.KeyFrames.Add(toFrame);

                Timeline.SetDesiredFrameRate(doubleAnimKeyFrames, 240);

                BeginAnimation(Canvas.LeftProperty, doubleAnimKeyFrames);
                BeginAnimation(OpacityProperty, FadeAnimation.FadeIn(1, 0.25));
            });
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Closing -= Window_Closing;
            e.Cancel = true;
            BeginAnimation(OpacityProperty, FadeAnimation.FadeTo(0.1, () => { Close(); }));
        }
    }
}
