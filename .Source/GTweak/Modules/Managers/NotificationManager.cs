using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using GTweak.Modules.Common;
using GTweak.Modules.Tweaks;
using GTweak.Windows;

namespace GTweak.Modules.Managers
{
    internal static class NotificationManager
    {
        internal enum NoticeAction { None, Logout, Restart }

        internal static readonly Dictionary<Enum, NoticeAction> ConfActions = new Dictionary<Enum, NoticeAction>()
        {
            [ConfidentialityToggle.HardwareConfigurationData] = NoticeAction.Restart,
            [ConfidentialityToggle.CovertDataCollectionServices] = NoticeAction.Restart,
            [ConfidentialityToggle.IntelTelemetry] = NoticeAction.Restart
        };

        internal static readonly Dictionary<Enum, NoticeAction> IntfActions = new Dictionary<Enum, NoticeAction>()
        {
            [InterfaceToggle.SystemButtonSize] = NoticeAction.Logout,
            [InterfaceToggle.CursorFlickerFrequency] = NoticeAction.Logout,
            [InterfaceToggle.ScrollbarSize] = NoticeAction.Logout,
            [InterfaceToggle.ContextMenuDelay] = NoticeAction.Logout,
            [InterfaceToggle.TaskbarPreviewDelay] = NoticeAction.Logout,
            [InterfaceToggle.IconOverlayBadges] = NoticeAction.Logout,
            [InterfaceToggle.ShortcutNameSuffix] = NoticeAction.Logout,
            [InterfaceToggle.CopilotRecall] = NoticeAction.Restart,
            [InterfaceToggle.SnapLayouts] = NoticeAction.Logout,
            [InterfaceToggle.AdaptiveBrightness] = NoticeAction.Restart
        };

        internal static readonly Dictionary<Enum, NoticeAction> SysActions = new Dictionary<Enum, NoticeAction>()
        {
            [SystemToggle.StickyKeysFilter] = NoticeAction.Logout,
            [SystemToggle.WindowsDefender] = NoticeAction.Restart,
            [SystemToggle.UserAccountControl] = NoticeAction.Restart,
            [SystemToggle.SecurityNotifications] = NoticeAction.Restart,
            [SystemToggle.RealtekAudioDelay] = NoticeAction.Restart,
            [SystemToggle.MemoryDiagnostics] = NoticeAction.Restart,
            [SystemToggle.NetworkProtocols] = NoticeAction.Restart,
            [SystemToggle.FileSystemCache] = NoticeAction.Restart,
            [SystemToggle.StartupDelay] = NoticeAction.Restart,
            [SystemToggle.WindowsFirewall] = NoticeAction.Restart,
            [SystemToggle.BackgroundApps] = NoticeAction.Restart,
            [SystemToggle.DynamicTickHpet] = NoticeAction.Restart,
            [SystemToggle.InsiderTasks] = NoticeAction.Restart,
            [SystemToggle.MultiPlaneOverlay] = NoticeAction.Restart
        };

        private static int _isNotificationOpen;

        internal static NotificationBuilder Show(string titleKey = "", string textKey = "")
        {
            string title = Application.Current?.TryFindResource($"title_{titleKey}_noty") as string ?? string.Empty;
            string text = Application.Current?.TryFindResource(textKey) as string ?? string.Empty;
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
                            RequiredAction = action
                        };

                        if (string.IsNullOrWhiteSpace(window.NoticeTitle) && string.IsNullOrWhiteSpace(window.NoticeText))
                        {
                            window.NoticeTitle = (Application.Current?.Resources)?["title_warn_noty"] as string ?? string.Empty;
                            window.NoticeText = (Application.Current?.Resources)?[action == NoticeAction.Logout ? "logout_noty" : "restart_noty"] as string ?? string.Empty;
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