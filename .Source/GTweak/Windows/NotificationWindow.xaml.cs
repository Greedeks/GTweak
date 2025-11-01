using GTweak.Utilities.Animation;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Managers;
using System;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Wpf.Ui.Controls;

namespace GTweak.Windows
{
    public partial class NotificationWindow : FluentWindow
    {
        private NotificationManager.NoticeAction _requiredAction = default;
        private TimerControlManager _timer = default;

        internal string NoticeTitle { set => Header.Text = value; get => Header.Text; }
        internal string NoticeText { set => MessageBody.Text = value; get => MessageBody.Text; }
        internal NotificationManager.NoticeAction RequiredAction { set => _requiredAction = value; get => _requiredAction; }

        public NotificationWindow()
        {
            InitializeComponent();

            Unloaded += delegate { _timer.Stop(); };
            Loaded += delegate
            {
                _timer = new TimerControlManager(TimeSpan.FromSeconds(3), TimerControlManager.TimerMode.CountDown, null, () => { Close(); });
                _timer.Start();
                ProgressTimer.BeginAnimation(RangeBase.ValueProperty, FactoryAnimation.CreateIn(0, 100, 4.1));
            };
        }

        private void ButtonClose_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => Close();

        private void Grid_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => CommandExecutor.RunCommand(RequiredAction == NotificationManager.NoticeAction.Logout ? @"/c logoff" : @"/c shutdown /r /t 0");

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (SettingsEngine.IsPlayingSound)
            {
                using SoundPlayer notificationSound = new SoundPlayer(Properties.Resources.Sound);
                notificationSound.Play();
            }

            Rect primaryMonitorArea = SystemParameters.WorkArea;

            Top = primaryMonitorArea.Bottom - Height - 10;
            Left = primaryMonitorArea.Right - Width - 10;

            DoubleAnimationUsingKeyFrames doubleAnimKeyFrames = new DoubleAnimationUsingKeyFrames();

            doubleAnimKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame(primaryMonitorArea.Right, TimeSpan.Zero)
            {
                EasingFunction = new QuadraticEase()
            });
            doubleAnimKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame(primaryMonitorArea.Right - Width - 10, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(300)))
            {
                EasingFunction = new QuadraticEase()
            });

            Timeline.SetDesiredFrameRate(doubleAnimKeyFrames, 120);
            BeginAnimation(Canvas.LeftProperty, doubleAnimKeyFrames);
        }
    }
}
