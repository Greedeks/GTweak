using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using GTweak.Utilities.Controls;
using GTweak.Windows;

namespace GTweak.Utilities.Managers
{
    internal static class NotificationManager
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
            ["TglButton20"] = NoticeAction.Restart,
            ["TglButton22"] = NoticeAction.Logout,
            ["TglButton23"] = NoticeAction.Restart,
        };

        internal static readonly Dictionary<string, NoticeAction> SysActions = new Dictionary<string, NoticeAction>()
        {
            ["TglButton2"] = NoticeAction.Logout,
            ["TglButton3"] = NoticeAction.Restart,
            ["TglButton4"] = NoticeAction.Restart,
            ["TglButton5"] = NoticeAction.Restart,
            ["TglButton7"] = NoticeAction.Restart,
            ["TglButton12"] = NoticeAction.Restart,
            ["TglButton13"] = NoticeAction.Restart,
            ["TglButton14"] = NoticeAction.Restart,
            ["TglButton15"] = NoticeAction.Restart,
            ["TglButton20"] = NoticeAction.Restart,
            ["TglButton23"] = NoticeAction.Restart,
            ["TglButton25"] = NoticeAction.Restart,
            ["TglButton27"] = NoticeAction.Restart
        };

        private static int _isNotificationOpen;
        internal static NotificationBuilder Show(string titleKey = "", string textKey = "")
        {
            string title = Application.Current.TryFindResource($"title_{titleKey}_noty") as string ?? string.Empty;
            string text = Application.Current.TryFindResource(textKey) as string ?? string.Empty;
            return new NotificationBuilder(title, text);
        }

        internal sealed class NotificationBuilder
        {
            private readonly string _title;
            private readonly string _text;
            private int _delayMs = 100;

            internal NotificationBuilder(string title, string text)
            {
                _title = title;
                _text = text;
            }

            internal NotificationBuilder WithDelay(int ms)
            {
                _delayMs = ms;
                return this;
            }

            internal void Perform(NoticeAction action = NoticeAction.None) => ShowNotification(action);
            internal void Logout() => ShowNotification(NoticeAction.Logout);
            internal void Restart() => ShowNotification(NoticeAction.Restart);

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
                            window.NoticeTitle = Application.Current.Resources["title_warn_noty"] as string;
                            window.NoticeText = Application.Current.Resources[action == NoticeAction.Logout ? "logout_noty" : "restart_noty"] as string;
                        }

                        window.Closed += (s, e) => Interlocked.Exchange(ref _isNotificationOpen, 0);

                        await Task.Delay(_delayMs).ContinueWith(_ =>
                        {
                            if (window != null && !window.IsVisible)
                            {
                                window.Show();
                            }
                        }, TaskScheduler.FromCurrentSynchronizationContext());
                    });
                }
            }
        }
    }

}