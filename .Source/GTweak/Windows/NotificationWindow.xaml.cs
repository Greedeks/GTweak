using System;
using System.Media;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using GTweak.Utilities.Animation;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Managers;
using Wpf.Ui.Controls;

namespace GTweak.Windows
{
    public partial class NotificationWindow : FluentWindow
    {
        private NotificationManager.NoticeAction _requiredAction = default;
        private TimerControlManager _timer = default;
        private Rect primaryMonitorArea = SystemParameters.WorkArea;

        internal string NoticeTitle { set => Header.Text = value; get => Header.Text; }
        internal string NoticeText { set => MessageBody.Text = value; get => MessageBody.Text; }
        internal NotificationManager.NoticeAction RequiredAction { set => _requiredAction = value; get => _requiredAction; }

        public NotificationWindow()
        {
            InitializeComponent();

            SourceInitialized += delegate
            {
                Top = primaryMonitorArea.Bottom - Height - 10;
                Left = primaryMonitorArea.Right + 10;
            };

            Unloaded += delegate { _timer.Stop(); };
            Loaded += delegate
            {
                _timer = new TimerControlManager(TimeSpan.FromSeconds(3), TimerControlManager.TimerMode.CountDown, null, () => { Close(); });
                _timer.Start();
                ProgressTimer.BeginAnimation(RangeBase.ValueProperty, FactoryAnimation.CreateIn(0, 100, 4.1));
            };
        }

        private void ButtonClose_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => Close();

        private void Grid_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (RequiredAction != NotificationManager.NoticeAction.None)
            {
                CommandExecutor.RunCommand(RequiredAction == NotificationManager.NoticeAction.Logout ? @"/c logoff" : @"/c shutdown /r /t 0");
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (SettingsEngine.IsPlayingSound)
            {
                using SoundPlayer notificationSound = new SoundPlayer(Properties.Resources.Sound);
                notificationSound.Play();
            }

            BeginAnimation(LeftProperty, FactoryAnimation.CreateIn(primaryMonitorArea.Right + 10, primaryMonitorArea.Right - Width - 10, 0.3, useCubicEase: true));
        }
    }
}
