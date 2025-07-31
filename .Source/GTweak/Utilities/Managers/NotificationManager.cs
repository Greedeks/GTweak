using GTweak.Utilities.Controls;
using GTweak.Windows;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace GTweak.Utilities.Managers
{
    internal class NotificationManager
    {
        internal enum NoticeAction { None, Logout, Restart }

        internal static readonly Dictionary<string, NoticeAction> ConfActions = new Dictionary<string, NoticeAction>()
        {
            ["TglButton8"] = NoticeAction.Restart,
            ["TglButton15"] = NoticeAction.Restart
        };

        internal static readonly Dictionary<string, NoticeAction> IntfActions = new Dictionary<string, NoticeAction>()
        {
            ["TglButton1"] = NoticeAction.Logout,
            ["TglButton2"] = NoticeAction.Logout,
            ["TglButton3"] = NoticeAction.Logout,
            ["TglButton4"] = NoticeAction.Logout,
            ["TglButton5"] = NoticeAction.Logout,
            ["TglButton10"] = NoticeAction.Logout,
            ["TglButton11"] = NoticeAction.Logout,
            ["TglButton12"] = NoticeAction.Logout,
            ["TglButton22"] = NoticeAction.Restart,
            ["TglButton20"] = NoticeAction.Restart
        };

        internal static readonly Dictionary<string, NoticeAction> SysActions = new Dictionary<string, NoticeAction>()
        {
            ["TglButton2"] = NoticeAction.Logout,
            ["TglButton7"] = NoticeAction.Restart,
            ["TglButton8"] = NoticeAction.Restart,
            ["TglButton9"] = NoticeAction.Restart,
            ["TglButton10"] = NoticeAction.Restart,
            ["TglButton12"] = NoticeAction.Restart,
            ["TglButton13"] = NoticeAction.Restart,
            ["TglButton14"] = NoticeAction.Restart,
            ["TglButton15"] = NoticeAction.Restart,
            ["TglButton20"] = NoticeAction.Restart,
            ["TglButton23"] = NoticeAction.Restart,
            ["TglButton25"] = NoticeAction.Restart,
            ["TglButton27"] = NoticeAction.Restart
        };

        private readonly int _delayMs;

        internal NotificationManager(int delayMs = 100) => _delayMs = delayMs;

        internal NotificationBuilder Show(string titleKey = "", string textKey = "") => new NotificationBuilder(_delayMs, (string)Application.Current.TryFindResource($"title_{titleKey}_notification") ?? string.Empty, (string)Application.Current.TryFindResource(textKey) ?? string.Empty);
    }

    internal sealed class NotificationBuilder : NotificationManager
    {
        private static int _isNotificationOpen;
        private readonly int _delayMs;
        private readonly string _title = string.Empty, _text = string.Empty;

        internal NotificationBuilder(int delayMs, string title, string text)
        {
            _delayMs = delayMs;
            _title = title;
            _text = text;
        }

        internal void None() => ShowNotification(NoticeAction.None);
        internal void Logout() => ShowNotification(NoticeAction.Logout);
        internal void Restart() => ShowNotification(NoticeAction.Restart);
        internal void Execute(NoticeAction action) => ShowNotification(action);

        private async void ShowNotification(NoticeAction action)
        {
            if (SettingsEngine.IsViewNotification && Interlocked.CompareExchange(ref _isNotificationOpen, 1, 0) == 0)
            {
                await Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    NotificationWindow window = new NotificationWindow
                    {
                        NoticeTitle = _title,
                        NoticeText = _text,
                        RequiredAction = action
                    };

                    if (string.IsNullOrWhiteSpace(window.NoticeTitle) && string.IsNullOrWhiteSpace(window.NoticeText))
                    {
                        window.NoticeTitle = (string)Application.Current.Resources["title_warn_notification"];
                        window.NoticeText = (string)Application.Current.Resources[window.RequiredAction == NoticeAction.Logout ? "logout_notification" : "restart_notification"];
                    }

                    window.Closed += (s, e) => Interlocked.Exchange(ref _isNotificationOpen, 0);

                    await Task.Delay(_delayMs).ContinueWith(_ => { if (window != null && !window.IsVisible) window.Show(); }, TaskScheduler.FromCurrentSynchronizationContext());
                });
            }
        }
    }
}