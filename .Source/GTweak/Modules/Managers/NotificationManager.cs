using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using GTweak.Modules.Common;
using GTweak.Windows;

namespace GTweak.Modules.Managers
{
    internal static class NotificationManager
    {
        internal enum AlertType { None, Logout, Restart }

        private static volatile int _isNotificationOpen;

        internal static NotificationBuilder Info(string textKey) => Builder("info", textKey);
        internal static NotificationBuilder Warn(string textKey) => Builder("warn", textKey);
        internal static NotificationBuilder Default() => Builder(string.Empty, string.Empty);

        private static NotificationBuilder Builder(string titleKey, string textKey)
        {
            string title = string.IsNullOrEmpty(titleKey) ? string.Empty : Application.Current?.TryFindResource($"title_{titleKey}_noty") as string ?? string.Empty;
            string text = string.IsNullOrEmpty(textKey) ? string.Empty : Application.Current?.TryFindResource(textKey) as string ?? string.Empty;
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

            internal void Perform(AlertType alert = AlertType.None) => ShowNotification(alert);
            internal void Logout() => ShowNotification(AlertType.Logout);
            internal void Restart() => ShowNotification(AlertType.Restart);

            private async void ShowNotification(AlertType type)
            {
                if (GlobalOptions.IsViewNotification && Interlocked.CompareExchange(ref _isNotificationOpen, 1, 0) == 0)
                {
                    Dispatcher dispatcher = Application.Current?.Dispatcher;
                    if (dispatcher == null)
                    {
                        Interlocked.Exchange(ref _isNotificationOpen, 0);
                        return;
                    }

                    await dispatcher.InvokeAsync(async () =>
                    {
                        NotificationWindow window = new NotificationWindow
                        {
                            NoticeTitle = _title,
                            NoticeText = _text,
                            AlertType = type
                        };

                        if (string.IsNullOrWhiteSpace(window.NoticeTitle) && string.IsNullOrWhiteSpace(window.NoticeText))
                        {
                            window.NoticeTitle = (Application.Current?.Resources)?["title_warn_noty"] as string ?? string.Empty;
                            window.NoticeText = (Application.Current?.Resources)?[type == AlertType.Logout ? "logout_noty" : "restart_noty"] as string ?? string.Empty;
                        }

                        window.Closed += delegate { Interlocked.Exchange(ref _isNotificationOpen, 0); };

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