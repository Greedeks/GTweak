using GTweak.Utilities.Controls;
using GTweak.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GTweak.Utilities.Helpers.Managers
{
    internal sealed class NotificationManager
    {
        internal static readonly Dictionary<string, string> ConfActions = new[]
        {
            new { Button = "TglButton8", Action = "restart" },
            new { Button = "TglButton15", Action = "restart" }
        }.ToDictionary(x => x.Button, x => x.Action);

        internal static readonly Dictionary<string, string> IntfActions = new[]
        {
            new { Button = "TglButton1", Action = "logout" },
            new { Button = "TglButton2", Action = "logout" },
            new { Button = "TglButton3", Action = "logout" },
            new { Button = "TglButton4", Action = "logout" },
            new { Button = "TglButton5", Action = "logout" },
            new { Button = "TglButton10", Action = "logout" },
            new { Button = "TglButton11", Action = "logout" },
            new { Button = "TglButton12", Action = "logout" },
            new { Button = "TglButton22", Action = "restart" },
            new { Button = "TglButton20", Action = "restart" }
        }.ToDictionary(x => x.Button, x => x.Action);

        internal static readonly Dictionary<string, string> SysActions = new[]
        {
            new { Button = "TglButton2", Action = "logout" },
            new { Button = "TglButton7", Action = "restart" },
            new { Button = "TglButton9", Action = "restart" },
            new { Button = "TglButton10", Action = "restart" },
            new { Button = "TglButton12", Action = "restart" },
            new { Button = "TglButton13", Action = "restart" },
            new { Button = "TglButton14", Action = "restart" },
            new { Button = "TglButton15", Action = "restart" },
            new { Button = "TglButton20", Action = "restart" },
            new { Button = "TglButton23", Action = "restart" }
        }.ToDictionary(x => x.Button, x => x.Action);

        private readonly int _delayMs;
        private static bool _isNotificationOpen = false;

        internal NotificationManager(int delayMs = 100) => _delayMs = delayMs;

        internal async void Show(string action, string titleKey = "", string textKey = "")
        {
            if (SettingsRepository.IsViewNotification && !_isNotificationOpen)
            {
                await Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    _isNotificationOpen = true;

                    NotificationWindow window = new NotificationWindow
                    {
                        ActionNotice = action,
                        TitleNotice = (string)Application.Current.TryFindResource($"title_{titleKey}_notification") ?? string.Empty,
                        TextNotice = (string)Application.Current.TryFindResource(textKey) ?? string.Empty,
                    };

                    switch (window.ActionNotice)
                    {
                        case "logout":
                            if (string.IsNullOrWhiteSpace(window.TitleNotice) || string.IsNullOrWhiteSpace(window.TextNotice))
                            {
                                window.TitleNotice = (string)Application.Current.Resources["title_warn_notification"];
                                window.TextNotice = (string)Application.Current.Resources["logout_notification"];
                            }
                            break;
                        case "restart":
                            if (string.IsNullOrWhiteSpace(window.TitleNotice) || string.IsNullOrWhiteSpace(window.TextNotice))
                            {
                                window.TitleNotice = (string)Application.Current.Resources["title_warn_notification"];
                                window.TextNotice = (string)Application.Current.Resources["restart_notification"];
                            }
                            break;
                    }

                    window.Closed += (s, e) => { _isNotificationOpen = false; };

                    await Task.Delay(_delayMs).ContinueWith(_ => window.Show(), TaskScheduler.FromCurrentSynchronizationContext());
                });

            }
        }
    }
}
