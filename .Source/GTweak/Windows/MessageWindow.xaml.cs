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
        public enum MessageWindowType
        {
            Warning,
            NotSupported,
            NotAdmin
        }

        private TimerControlManager _timer = default;
        public MessageWindow(MessageWindowType type = MessageWindowType.Warning)
        {
            InitializeComponent();
            InitializeMessageContent(type);

            Loaded += delegate
            {
                _timer = new TimerControlManager(TimeSpan.FromSeconds(4), TimerControlManager.TimerMode.CountDown, time =>
                {
                    BtnAccept.Content = $"{new Regex("[(05)(04)(03)(02)]").Replace(BtnAccept.Content.ToString(), "")}({time:ss})";
                }, () => Application.Current.Shutdown());
                _timer.Start();
            };
            Unloaded += delegate { _timer.Stop(); };
        }

        private void InitializeMessageContent(MessageWindowType type)
        {
            switch (type)
            {
                case MessageWindowType.NotAdmin:
                    UpdateMessageElements(SymbolRegular.ShieldLock24, "title_admin_message", "part1_admin_message", "part2_admin_message", "part3_admin_message");
                    break;
                case MessageWindowType.NotSupported:
                    UpdateMessageElements(SymbolRegular.ErrorCircle24, "title_nosupport_message", "part1_nosupport_message", "part2_nosupport_message", "part3_nosupport_message");
                    break;
                default:
                    UpdateMessageElements(SymbolRegular.Warning24, "title_message", "part1_message", "part2_message", "part3_message");
                    break;
            }
        }

        private void UpdateMessageElements(SymbolRegular symbol, string titleKey, string p1Key, string p2Key, string p3Key)
        {
            MsgIcon.Symbol = symbol;
            TxtTitle.Text = (string)FindResource(titleKey) ?? string.Empty;
            Part1.Text = (string)FindResource(p1Key) ?? string.Empty;
            Part2.Text = (string)FindResource(p2Key) ?? string.Empty;
            Part3.Text = (string)FindResource(p3Key) ?? string.Empty;
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
