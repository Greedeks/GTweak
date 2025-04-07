using GTweak.Windows;
using System.Threading.Tasks;
using System.Windows;

namespace GTweak.Utilities.Controls
{
    internal sealed class ViewNotification
    {
        private static bool _isAlreadyLaunch = false;
        private readonly int _msec = default;

        internal ViewNotification(int delay = 100) => _msec = delay;

        internal async void Show(string action, string tittle = "", string content = "")
        {
            if (SettingsRepository.IsViewNotification && !_isAlreadyLaunch)
            {
                await Application.Current.Dispatcher.InvokeAsync(async delegate
                {
                    _isAlreadyLaunch = true;

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

                    await Task.Delay(_msec);
                    notificationWindow.Show();
                    notificationWindow.Closed += delegate { _isAlreadyLaunch = false; };
                });
            }
        }
    }
}
