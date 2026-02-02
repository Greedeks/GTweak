using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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
        private static readonly Mutex _mutex = new Mutex(false, "GTweak");

        internal static void CheckingApplicationCopies()
        {
            if (!_mutex.WaitOne(150, false))
            {
                using (Mutex mutex = new Mutex(false, @"Global\" + "GTweak"))
                {
                    if (mutex.WaitOne(150, false))
                    {
                        Application.Current.Dispatcher.Invoke(() => { new MessageWindow().ShowDialog(); });
                    }
                    else
                    {
                        Environment.Exit(0);
                    }
                }
                using (Process process = Process.GetProcesses().FirstOrDefault(p => p.ProcessName == Process.GetCurrentProcess().ProcessName))
                {
                    if (process != null)
                    {
                        IntPtr handle = process.MainWindowHandle;
                        ShowWindow(handle, 1);
                        SetForegroundWindow(handle);
                    }
                }
                Environment.Exit(0);
            }
        }

        internal static async Task CheckingSystemRequirements()
        {
            await Task.Run(() => new SystemDiagnostics().GetOperatingSystemInfo());

            if ((HardwareData.OS.IsWin11 || HardwareData.OS.IsWin10) && HardwareData.OS.Build.CompareTo(18362.116m) >= 0)
            {
                return;
            }

            new MessageWindow(true).ShowDialog();
        }

        internal static void CheckingDefenderExclusions() => CommandExecutor.RunCommandAsTrustedInstaller($@"$ErrorActionPreference = 'Stop'; $target = '{SettingsEngine.currentLocation}'; try {{ $mp = Get-MpPreference; if ($mp.ExclusionProcess -notcontains $target) {{ Add-MpPreference -ExclusionProcess $target }}; if ($mp.ExclusionPath -notcontains $target) {{ Add-MpPreference -ExclusionPath $target }} }} catch {{}}", true);
    }
}
