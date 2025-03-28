﻿using GTweak.Utilities.Configuration;
using GTweak.Windows;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace GTweak.Utilities.Control
{
    internal sealed class BlockRunTweaker
    {
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr handle, int cmdShow);
        [DllImport("user32.dll")]
        private static extern int SetForegroundWindow(IntPtr handle);
        private static readonly Mutex mutex = new Mutex(false, "GTweak");

        internal static void CheckingApplicationCopies()
        {
            if (!mutex.WaitOne(150, false))
            {
                using (Mutex mutex = new Mutex(false, @"Global\" + "GTweak"))
                {
                    if (mutex.WaitOne(150, false))
                        Application.Current.Dispatcher.Invoke(() => { new MessageWindow().ShowDialog(); });
                    else
                        Environment.Exit(0);
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

        internal static void CheckingSystemRequirements()
        {
            Task.Run(delegate { new SystemDiagnostics().GetOperatingSystemInfo(); }).Wait();

            if ((SystemDiagnostics.IsWindowsVersion[11] || SystemDiagnostics.IsWindowsVersion[10]) && SystemDiagnostics.WindowsBuildVersion.CompareTo("18362.116") >= 0) return;
            new MessageWindow(true).ShowDialog();
        }
    }
}
