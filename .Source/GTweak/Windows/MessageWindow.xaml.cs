using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using GTweak.Utilities.Managers;
using Wpf.Ui.Controls;

namespace GTweak.Windows
{
    public partial class MessageWindow : FluentWindow
    {
        private TimerControlManager _timer = default;
        public MessageWindow(bool isViolationSystem = false)
        {
            InitializeComponent();

            if (isViolationSystem)
            {
                NotSupportContent.Visibility = Visibility.Visible;
                WarningContent.Visibility = Visibility.Collapsed;
            }

            Unloaded += delegate { _timer.Stop(); };
            Loaded += delegate
            {
                _timer = new TimerControlManager(TimeSpan.FromSeconds(4), TimerControlManager.TimerMode.CountDown, time =>
                {
                    BtnAccept.Content = $"{new Regex("[(05)(04)(03)(02)]").Replace(BtnAccept.Content.ToString(), "")}({time:ss})";
                }, () => Application.Current.Shutdown());
                _timer.Start();
            };
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e?.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void BtnAccept_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e?.LeftButton == MouseButtonState.Pressed)
            {
                Application.Current.Shutdown();
            }
        }

    }
}
