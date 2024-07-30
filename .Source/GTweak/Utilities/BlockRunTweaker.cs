using GTweak.Windows;
using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace GTweak.Utilities
{
    internal sealed class BlockRunTweaker
    {
        [DllImport("user32.dll")]
        private static extern bool _ShowWindow(IntPtr handle, int cmdShow);
        [DllImport("user32.dll")]
        private static extern int _SetForegroundWindow(IntPtr handle);
        private static readonly Mutex mutex = new Mutex(false, "GTweak");

        internal static void CheckingApplicationCopies()
        {
            if (!mutex.WaitOne(150, false))
            {
                using (Mutex mutex = new Mutex(false, @"Global\" + "GTweak"))
                {
                    if (mutex.WaitOne(0, false))
                        new MessageWindow().ShowDialog();
                }
                string _processName = Process.GetCurrentProcess().ProcessName;
                Process process = Process.GetProcesses().FirstOrDefault(p => p.ProcessName == _processName);
                if (process != null)
                {
                    IntPtr handle = process.MainWindowHandle;
                    _ShowWindow(handle, 1);
                    _SetForegroundWindow(handle);
                }
                process?.Close();
                process?.Dispose();
            }
        }

        internal static void CheckingSystemRequirements()
        {
            EnumerationOptions optionsObj = new EnumerationOptions { ReturnImmediately = true };

            Parallel.Invoke(() =>
            {
                foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "select Caption from Win32_OperatingSystem", optionsObj).Get())
                    SystemData.СomputerСonfiguration.clientWinVersion = Convert.ToString(managementObj["Caption"]);;
            });

            if (!SystemData.СomputerСonfiguration.clientWinVersion.Contains("11") && !SystemData.СomputerСonfiguration.clientWinVersion.Contains("10"))
            {
                MessageWindow messageWindow = new MessageWindow(true);
                messageWindow.ShowDialog();
            }
        }
    }
}
