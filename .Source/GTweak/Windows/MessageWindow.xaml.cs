using GTweak.Utilities.Animation;
using GTweak.Utilities.Managers;
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace GTweak.Windows
{
    public partial class MessageWindow
    {


        public MessageWindow(bool isViolationSystem = false)
        {
            InitializeComponent();

            TimerControlManager timer = new TimerControlManager(TimeSpan.FromSeconds(4), TimerControlManager.TimerMode.CountDown, time =>
            {
                BtnAccept.Content = $"{new Regex("[(05)(04)(03)(02)]").Replace(BtnAccept.Content.ToString(), "")}({time:ss})";
            }, () => Application.Current.Shutdown());
            timer.Start();

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
            BeginAnimation(OpacityProperty, FactoryAnimation.CreateTo(0.1, () => { Close(); }));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) => BeginAnimation(OpacityProperty, FactoryAnimation.CreateIn(0, 1, 0.2));
    }
}
