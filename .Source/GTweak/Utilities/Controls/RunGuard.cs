using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using GTweak.Utilities.Configuration;
using GTweak.Utilities.Helpers;
using GTweak.Windows;

namespace GTweak.Utilities.Controls
{
    internal static class RunGuard
    {
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr handle, int cmdShow);
        [DllImport("user32.dll")]
        private static extern int SetForegroundWindow(IntPtr handle);

        private static readonly Mutex _mutex = new Mutex(false, @"GTweak"), _msgMutex = new Mutex(false, @"Global\GTweak");

        internal static void CheckingSingleInstance()
        {
            if (!_mutex.WaitOne(0, false))
            {
                Process existing = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).FirstOrDefault(p => p.Id != Process.GetCurrentProcess().Id);
                if (existing != null)
                {
                    IntPtr handle = existing.MainWindowHandle;
                    if (handle != IntPtr.Zero)
                    {
                        ShowWindow(handle, 9);
                        SetForegroundWindow(handle);
                    }
                }

                if (_msgMutex.WaitOne(0, false))
                {
                    try { Application.Current.Dispatcher.Invoke(() => { new MessageWindow().ShowDialog(); }); }
                    finally { _msgMutex.ReleaseMutex(); }
                }
                Environment.Exit(0);
            }
        }

        internal static async Task CheckingSystemRequirements()
        {
            await Task.Run(() => new HardwareProvider().GetOperatingSystemInfo());

            if ((HardwareData.OS.IsWin11 || HardwareData.OS.IsWin10) && HardwareData.OS.Build.CompareTo(18362.116m) >= 0)
            {
                return;
            }

            new MessageWindow(MessageWindow.MessageWindowType.NotSupported).ShowDialog();
        }

        internal static void CheckingAdministratorPrivileges()
        {
            using WindowsIdentity identity = WindowsIdentity.GetCurrent();

            if (new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator))
            {
                return;
            }

            new MessageWindow(MessageWindow.MessageWindowType.NotAdmin).ShowDialog();
        }
        internal static void CheckingDefenderExclusions() => CommandExecutor.RunCommandAsTrustedInstaller($@"$ErrorActionPreference = 'Stop'; $target = '{SettingsEngine.currentLocation}'; try {{ $mp = Get-MpPreference; if ($mp.ExclusionProcess -notcontains $target) {{ Add-MpPreference -ExclusionProcess $target }}; if ($mp.ExclusionPath -notcontains $target) {{ Add-MpPreference -ExclusionPath $target }} }} catch {{}}", true);
    }
}
