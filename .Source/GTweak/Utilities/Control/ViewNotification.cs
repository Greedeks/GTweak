using GTweak.Windows;
using System.Threading.Tasks;
using System.Windows;

namespace GTweak.Utilities.Control
{
    internal sealed class ViewNotification
    {
        private static bool isAlreadyLaunch = false;
        private readonly int msec = default;

        internal ViewNotification(int delay = 100) => msec = delay;

        internal async void Show(string action, string tittle = "", string content = "")
        {
            if (SettingsRepository.IsViewNotification && !isAlreadyLaunch)
            {
                await Application.Current.Dispatcher.InvokeAsync(async delegate
                {
                    isAlreadyLaunch = true;

                    NotificationWindow notificationWindow = new NotificationWindow
                    {
                        TitleNotice = tittle switch
                        {
                            "warn" => (string)Application.Current.Resources["title_warn_notification"],
                            "info" => (string)Application.Current.Resources["title_info_notification"],
                            _ => string.Empty,
                        },
                        TextNotice = (string)Application.Current.Resources[content],
                        ActionNotice = action,
                    };

                    await Task.Delay(msec);
                    notificationWindow.Show();
                    notificationWindow.Closed += delegate { isAlreadyLaunch = false; };
                });
            }
        }
    }
}
