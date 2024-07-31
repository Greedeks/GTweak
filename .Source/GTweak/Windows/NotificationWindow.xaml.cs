using GTweak.Utilities;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace GTweak.Windows
{
    public partial class NotificationWindow
    {
        private readonly DispatcherTimer timer;
        private TimeSpan time = TimeSpan.FromSeconds(3);
        private readonly MediaPlayer _mediaPlayer = new MediaPlayer();

        private string _action = string.Empty;
        internal string TitleNotice { set => TitleSet.Text = value; get => TitleSet.Text; }
        internal string TextNotice { set => TextSet.Text = value; get => TextSet.Text; }
        internal string ActionChoice { set => _action = value; get => _action; }

        public NotificationWindow()
        {
            InitializeComponent();

            ImageBody.Source ??= new BitmapImage(new Uri(Settings.PathIcon));

            App.ViewLang();

            timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                if (time == TimeSpan.Zero) { timer.Stop(); this.Close(); }
                time = time.Add(TimeSpan.FromSeconds(-1));
            }, Application.Current.Dispatcher);

            timer.Start();
        }

        private void BtnExit_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.Close();
        }

        private void ActionSelection_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                switch (ActionChoice)
                {
                    case "logout":
                        Process.Start("logoff");
                        break;
                    case "restart":
                        Process.Start("shutdown", "/r /t 0");
                        break;
                }
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            Parallel.Invoke(() =>
            {
                if (Settings.IsSoundNotification)
                {
                    _mediaPlayer.Open(new Uri(Settings.PathSound));
                    _mediaPlayer.Volume = Settings.VolumeNotification / 100.0f;
                    _mediaPlayer.Play();
                }
            });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            switch (ActionChoice)
            {
                case "logout":
                    if (TitleNotice == string.Empty || TextNotice == string.Empty)
                    {
                        TitleNotice = (string)FindResource("title0_notification");
                        TextNotice = (string)FindResource("logout_notification");
                    }
                    break;
                case "restart":
                    if (TitleNotice == string.Empty || TextNotice == string.Empty)
                    {
                        TitleNotice = (string)FindResource("title0_notification");
                        TextNotice = (string)FindResource("restart_notification");
                    }
                    break;
            }

            Rect primaryMonitorArea = SystemParameters.WorkArea;
            Top = primaryMonitorArea.Bottom - Height - 10;

            DoubleAnimationUsingKeyFrames doubleAnimKeyFrames = new DoubleAnimationUsingKeyFrames();

            EasingDoubleKeyFrame fromFrame = new EasingDoubleKeyFrame(primaryMonitorArea.Right)
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0))
            };

            EasingDoubleKeyFrame toFrame = new EasingDoubleKeyFrame(primaryMonitorArea.Right - Width - 10)
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(120))
            };

            doubleAnimKeyFrames.KeyFrames.Add(fromFrame);
            doubleAnimKeyFrames.KeyFrames.Add(toFrame);

            DoubleAnimation doubleAnim = new DoubleAnimation()
            {
                From = 0,
                To = 1,
                SpeedRatio = 3,
                Duration = TimeSpan.FromSeconds(0.8)
            };

            Timeline.SetDesiredFrameRate(doubleAnim, 400);
            Timeline.SetDesiredFrameRate(doubleAnimKeyFrames, 400);
            BeginAnimation(Canvas.LeftProperty, doubleAnimKeyFrames);
            BeginAnimation(OpacityProperty, doubleAnim);


            Left = primaryMonitorArea.Right - Width - 10;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Closing -= Window_Closing;
            e.Cancel = true;
            DoubleAnimation doubleAnimation = new DoubleAnimation(0, TimeSpan.FromSeconds(0.1));
            doubleAnimation.Completed += (s, _) => { Close(); };
            Timeline.SetDesiredFrameRate(doubleAnimation, 400);
            BeginAnimation(OpacityProperty, doubleAnimation);
        }
    }
}
