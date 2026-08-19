using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using GTweak.Modules.Common;
using GTweak.Modules.Tweaks;

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

        internal enum ExplorerAction { None, Refresh, Restart }

        internal static readonly Dictionary<Enum, ExplorerAction> IntfActions = new Dictionary<Enum, ExplorerAction>()
        {
            [InterfaceCheckbox.ExplorerHome] = ExplorerAction.Restart,
            [InterfaceCheckbox.ExplorerGallery] = ExplorerAction.Restart,
            [InterfaceCheckbox.ExplorerOneDrive] = ExplorerAction.Restart,
            [InterfaceCheckbox.FolderObjects3D] = ExplorerAction.Restart,
            [InterfaceCheckbox.FolderDesktop] = ExplorerAction.Restart,
            [InterfaceCheckbox.FolderDownloads] = ExplorerAction.Restart,
            [InterfaceCheckbox.FolderDocuments] = ExplorerAction.Restart,
            [InterfaceCheckbox.FolderPictures] = ExplorerAction.Restart,
            [InterfaceCheckbox.FolderMusic] = ExplorerAction.Restart,
            [InterfaceCheckbox.FolderVideo] = ExplorerAction.Restart,
            [InterfaceCheckbox.HiddenFiles] = ExplorerAction.Restart,
            [InterfaceCheckbox.SystemFiles] = ExplorerAction.Restart,
            [InterfaceCheckbox.FileExtensions] = ExplorerAction.Restart,
            [InterfaceCheckbox.EmptyDrives] = ExplorerAction.Restart,
            [InterfaceCheckbox.IconComputer] = ExplorerAction.Refresh,
            [InterfaceCheckbox.IconNetwork] = ExplorerAction.Refresh,
            [InterfaceCheckbox.IconRecycleBin] = ExplorerAction.Refresh,
            [InterfaceCheckbox.IconControlPanel] = ExplorerAction.Refresh,
            [InterfaceCheckbox.IconUserFiles] = ExplorerAction.Refresh,
            [InterfaceCheckbox.IconOneDrive] = ExplorerAction.Refresh,
            [InterfaceToggle.TaskbarDarkTheme] = ExplorerAction.Restart,
            [InterfaceToggle.AppDarkTheme] = ExplorerAction.Restart,
            [InterfaceToggle.TaskbarAlignment] = ExplorerAction.Restart,
            [InterfaceToggle.StartMenuLayout] = ExplorerAction.Restart,
            [InterfaceToggle.CompactContextMenu] = ExplorerAction.Restart,
            [InterfaceToggle.TaskbarEndTask] = ExplorerAction.Restart,
            [InterfaceToggle.TaskbarIconsButtons] = ExplorerAction.Restart,
            [InterfaceToggle.BingStartSearch] = ExplorerAction.Restart
        };

        internal static readonly Dictionary<string, ExplorerAction> PackageActions = new Dictionary<string, ExplorerAction>()
        {
            ["Widgets"] = ExplorerAction.Restart,
            ["Edge"] = ExplorerAction.Restart
        };

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
                        await Task.Delay(500, token);
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
                                try { process.Kill(); }
                                finally { process.Dispose(); }
                            }

                            try { capturedActions?.Invoke(); }
                            catch (Exception ex) { ErrorLogger.LogDebug(ex); }

                            using Process launchExplorer = new Process();
                            launchExplorer.StartInfo.FileName = PathTargets.Executable.Explorer;
                            launchExplorer.StartInfo.Arguments = "/factory,{EFD469A7-7E0A-4517-8B39-45873948DA31}";
                            launchExplorer.StartInfo.UseShellExecute = true;
                            launchExplorer.Start();
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
    }
}
