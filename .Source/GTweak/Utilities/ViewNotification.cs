﻿using GTweak.Windows;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace GTweak.Utilities
{
    internal sealed class ViewNotification
    {
        private static bool isAlreadyLaunch = false;

        internal async void Show(string action, string tittle = "", string content = "")
        {
            if (Settings.IsViewNotification)
            {
                Thread _thread = new Thread(() => new Settings().CheckingTempFiles()) { IsBackground = true };
                _thread.Start();

                await Task.Delay(200);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    Parallel.Invoke(async delegate
                    {
                        NotificationWindow notificationWindow = new NotificationWindow();

                        if (notificationWindow.IsLoaded || isAlreadyLaunch)
                            return;

                        isAlreadyLaunch = true;

                        notificationWindow = new NotificationWindow
                        {
                            TitleNotice = tittle switch
                            {
                                "warn" => (string)Application.Current.Resources["title_warn_notification"],
                                "info" => (string)Application.Current.Resources["title_info_notification"],
                                _ => string.Empty,
                            },
                            TextNotice = content,
                            ActionNotice = action,
                        };
                        await Task.Delay(100);
                        notificationWindow.Show();
                        notificationWindow.Closed += delegate { isAlreadyLaunch = false; };
                    });
                });
            }
        }
    }
}
