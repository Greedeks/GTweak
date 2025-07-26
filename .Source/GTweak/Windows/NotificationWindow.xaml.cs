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
        private string clickAction = string.Empty;
        internal string TitleNotice { set => TitleSet.Text = value; get => TitleSet.Text; }
        internal string TextNotice { set => TextSet.Text = value; get => TextSet.Text; }
        internal string ActionNotice { set => clickAction = value; get => clickAction; }

        public NotificationWindow()
        {
            InitializeComponent();

            TimerControlManager timer = new TimerControlManager(TimeSpan.FromSeconds(3), TimerControlManager.TimerMode.CountDown, null, () => { Close(); });
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
            BeginAnimation(OpacityProperty, FactoryAnimation.CreateTo(0.1, () => { Close(); }));
        }
    }
}
