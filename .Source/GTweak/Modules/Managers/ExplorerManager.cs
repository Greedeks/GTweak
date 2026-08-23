using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using GTweak.Modules.Common;

namespace GTweak.Modules.Managers
{
    internal class ExplorerManager
    {
        [DllImport("shell32.dll")]
        static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);
        const uint SHCNE_ASSOCCHANGED = 0x08000000;
        const uint SHCNF_FLUSH = 0x1000;

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint Msg, UIntPtr wParam, string lParam, uint fuFlags, uint uTimeout, out UIntPtr lpdwResult);
        const uint HWND_BROADCAST = 0xffff;
        const uint WM_SETTINGCHANGE = 0x001A;
        const uint SMTO_ABORTIFHUNG = 0x0002;

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        internal enum ShellType { None, Refresh, Restart }

        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private static readonly object _lockObj = new object();
        private static CancellationTokenSource _restartDelayCts;
        private static Action _pendingActions;

        internal static void Restart(Action action = null)
        {
            lock (_lockObj)
            {
                if (action != null)
                {
                    _pendingActions += action;
                }

                _restartDelayCts?.Cancel();
                _restartDelayCts = new CancellationTokenSource();
                var token = _restartDelayCts.Token;

                Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(300, token);
                        await _semaphore.WaitAsync(token);

                        try
                        {
                            Action capturedActions;
                            lock (_lockObj)
                            {
                                capturedActions = _pendingActions;
                                _pendingActions = null;
                            }

                            foreach (Process process in Process.GetProcessesByName("explorer"))
                            {
                                try
                                {
                                    process.Kill();
                                    process.WaitForExit(1000);
                                }
                                finally { process.Dispose(); }
                            }

                            DateTime deadline = DateTime.UtcNow.AddMilliseconds(500);
                            while (FindWindow("Progman", null) != IntPtr.Zero && DateTime.UtcNow < deadline)
                            {
                                await Task.Delay(50, CancellationToken.None);
                            }

                            try { capturedActions?.Invoke(); }
                            catch (Exception ex) { ErrorLogger.LogDebug(ex); }

                            Process.Start(new ProcessStartInfo
                            {
                                FileName = PathTargets.Executable.Explorer,
                                Arguments = "/factory,{EFD469A7-7E0A-4517-8B39-45873948DA31}",
                                UseShellExecute = true
                            });

                            int waitShellAlive = 20;
                            bool shellStarted = false;
                            while (waitShellAlive-- > 0)
                            {
                                await Task.Delay(100, CancellationToken.None);
                                if (FindWindow("Shell_TrayWnd", null) != IntPtr.Zero)
                                {
                                    shellStarted = true;
                                    break;
                                }
                            }

                            if (!shellStarted)
                            {
                                Process.Start(new ProcessStartInfo
                                {
                                    FileName = PathTargets.Executable.Explorer,
                                    UseShellExecute = true
                                });
                            }

                            await Task.Delay(500, CancellationToken.None);
                        }
                        finally { _semaphore.Release(); }
                    }
                    catch (Exception ex) { ErrorLogger.LogDebug(ex); }
                }, token);
            }
        }

        internal static void RefreshDesktop()
        {
            Task.Run(delegate
            {
                try
                {
                    SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_FLUSH, IntPtr.Zero, IntPtr.Zero);
                    SendMessageTimeout(new IntPtr((int)HWND_BROADCAST), WM_SETTINGCHANGE, UIntPtr.Zero, "TraySettings", SMTO_ABORTIFHUNG, 100, out UIntPtr result);
                }
                catch (Exception ex) { ErrorLogger.LogDebug(ex); }
            });
        }

        internal static void Handle(ShellType effect)
        {
            if (effect == ShellType.Restart)
            {
                Restart();
            }
            else if (effect == ShellType.Refresh)
            {
                RefreshDesktop();
            }
        }
    }
}