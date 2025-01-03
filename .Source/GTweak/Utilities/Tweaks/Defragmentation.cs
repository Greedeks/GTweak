using GTweak.Utilities.Control;
using GTweak.Utilities.Helpers;
using Microsoft.Win32;
using System.Windows;

namespace GTweak.Utilities.Tweaks
{
    internal sealed class Defragmentation : TaskSchedulerManager
    {
        private readonly string[] task = { @"Microsoft\Windows\Defrag\ScheduledDefrag" };
        internal void Disable()
        {
            if (IsTaskEnabled(task))
            {
                try
                {
                    SetTaskStateOwner(task, false);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\services\defragsvc", "Start", 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Dfrg\BootOptimizeFunction", "Enable", "N", RegistryValueKind.String);

                    new ViewNotification(300).Show("", "info", (string)Application.Current.Resources["success_defrag_notification"]);
                }
                catch { new ViewNotification(300).Show("", "warn", (string)Application.Current.Resources["error_defrag_notification"]); }
            }
            else
                new ViewNotification(300).Show("", "info", (string)Application.Current.Resources["warn_defrag_notification"]);
        }
    }
}
