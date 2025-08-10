using GTweak.Utilities.Animation;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Managers;
using System;
using System.ComponentModel;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace GTweak.Windows
{
    public partial class NotificationWindow
    {
        private NotificationManager.NoticeAction _requiredAction = default;
        private readonly TimerControlManager _timer = default;

        internal string NoticeTitle { set => Header.Text = value; get => Header.Text; }
        internal string NoticeText { set => MessageBody.Text = value; get => MessageBody.Text; }
        internal NotificationManager.NoticeAction RequiredAction { set => _requiredAction = value; get => _requiredAction; }

        public NotificationWindow()
        {
            InitializeComponent();

            _timer = new TimerControlManager(TimeSpan.FromSeconds(3), TimerControlManager.TimerMode.CountDown, null, () => { Close(); });
            _timer.Start();
        }

        private void BtnExit_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                Close();
        }

        private void ActionSelection_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && RequiredAction != NotificationManager.NoticeAction.None)
                CommandExecutor.RunCommand(RequiredAction == NotificationManager.NoticeAction.Logout ? @"/c logoff" : @"/c shutdown /r /t 0");
        }

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
            doubleAnimKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame(primaryMonitorArea.Right - Width - 10, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(200)))
            {
                EasingFunction = new QuadraticEase()
            });

            Timeline.SetDesiredFrameRate(doubleAnimKeyFrames, 240);
            BeginAnimation(Canvas.LeftProperty, doubleAnimKeyFrames);
            BeginAnimation(OpacityProperty, FactoryAnimation.CreateIn(0, 1, 0.25));
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Closing -= Window_Closing;
            e.Cancel = true;
            BeginAnimation(OpacityProperty, FactoryAnimation.CreateTo(0.1, () => { _timer.Stop(); Close(); }));
        }
    }
}
