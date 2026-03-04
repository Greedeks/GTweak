using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using GTweak.Utilities.Controls;

namespace GTweak.Utilities.Managers
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

        internal static readonly Dictionary<string, ExplorerAction> IntfActions = new Dictionary<string, ExplorerAction>()
        {
            ["Checkbox4"] = ExplorerAction.Restart,
            ["Checkbox5"] = ExplorerAction.Restart,
            ["Checkbox6"] = ExplorerAction.Restart,
            ["Checkbox7"] = ExplorerAction.Restart,
            ["Checkbox8"] = ExplorerAction.Refresh,
            ["Checkbox9"] = ExplorerAction.Refresh,
            ["Checkbox10"] = ExplorerAction.Refresh,
            ["Checkbox11"] = ExplorerAction.Refresh,
            ["Checkbox12"] = ExplorerAction.Refresh,
            ["Checkbox13"] = ExplorerAction.Refresh,
            ["TglButton5"] = ExplorerAction.Restart,
            ["TglButton6"] = ExplorerAction.Restart,
            ["TglButton11"] = ExplorerAction.Restart,
            ["TglButton12"] = ExplorerAction.Restart,
            ["TglButton13"] = ExplorerAction.Restart,
            ["TglButton17"] = ExplorerAction.Restart,
            ["TglButton19"] = ExplorerAction.Restart,
            ["TglButton23"] = ExplorerAction.Restart,
            ["TglButton24"] = ExplorerAction.Restart,
            ["TglButton27"] = ExplorerAction.Restart
        };

        internal static readonly Dictionary<string, ExplorerAction> PackageActions = new Dictionary<string, ExplorerAction>()
        {
            ["Widgets"] = ExplorerAction.Restart,
            ["Edge"] = ExplorerAction.Restart
        };

        internal static void Restart(Action action = null)
        {
            Task.Run(delegate
            {
                foreach (Process process in Process.GetProcesses())
                {
                    try
                    {
                        if (string.Compare(process.MainModule?.FileName, PathLocator.Executable.Explorer, StringComparison.OrdinalIgnoreCase) == 0 && Process.GetProcessesByName("explorer").Length != 0)
                        {
                            process.Kill();
                            action?.Invoke();
                        }
                    }
                    catch (Exception ex) { ErrorLogging.LogDebug(ex); }

                    if (Process.GetProcessesByName("explorer").Length == 0)
                    {
                        using Process launchExplorer = new Process();
                        launchExplorer.StartInfo.FileName = PathLocator.Executable.Explorer;
                        launchExplorer.StartInfo.Arguments = "/factory,{EFD469A7-7E0A-4517-8B39-45873948DA31}";
                        launchExplorer.StartInfo.UseShellExecute = true;
                        launchExplorer.Start();
                    }
                }
            });
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
                catch (Exception ex) { ErrorLogging.LogDebug(ex); }
            });
        }
    }
}
