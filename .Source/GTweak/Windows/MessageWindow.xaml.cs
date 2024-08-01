using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace GTweak.Windows
{
    public partial class MessageWindow
    {
        private readonly DispatcherTimer timer;
        private TimeSpan time = TimeSpan.FromSeconds(4);

        public MessageWindow(bool isViolationSystem = false)
        {
            InitializeComponent();

            App.ViewLang();

            try
            {
                timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
                {
                    BtnAccept.Content= $"{new Regex("[(05)(04)(03)(02)]").Replace(BtnAccept.Content.ToString(), "")}({time:ss})";
                    if (time == TimeSpan.Zero) { timer?.Stop(); Application.Current.Shutdown(); }
                    time = time.Add(TimeSpan.FromSeconds(-1));
                }, Application.Current.Dispatcher);

            } catch (Exception ex) { Debug.WriteLine(ex.Message.ToString()); }
            timer?.Start();

            if (isViolationSystem)
            {
                TextRun0.Text = (string)Application.Current.Resources["text0_nosupport_message"];
                TextRun1.Text = (string)Application.Current.Resources["text1_nosupport_message"];
                TextRun2.Text = (string)Application.Current.Resources["text2_nosupport_message"];
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
            DoubleAnimation doubleAnim = new DoubleAnimation(0, TimeSpan.FromSeconds(0.1));
            doubleAnim.Completed += (s, _) => { timer.Stop(); Application.Current.Shutdown(); };
            Timeline.SetDesiredFrameRate(doubleAnim, 400);
            BeginAnimation(OpacityProperty, doubleAnim);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DoubleAnimation doublAnim = new DoubleAnimation()
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.2)
            };
            Timeline.SetDesiredFrameRate(doublAnim, 400);
            BeginAnimation(OpacityProperty, doublAnim);
        }
    }
}
