using GTweak.Utilities.Managers;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace GTweak.Windows
{
    public partial class MessageWindow
    {
        private readonly TimerControlManager _timer = default;
        public MessageWindow(bool isViolationSystem = false)
        {
            InitializeComponent();

            _timer = new TimerControlManager(TimeSpan.FromSeconds(4), TimerControlManager.TimerMode.CountDown, time =>
            {
                BtnAccept.Content = $"{new Regex("[(05)(04)(03)(02)]").Replace(BtnAccept.Content.ToString(), "")}({time:ss})";
            }, () => Application.Current.Shutdown());
            _timer.Start();

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
    }
}
