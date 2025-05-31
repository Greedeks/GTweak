using GTweak.Utilities.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace GTweak.Utilities.Helpers.Managers
{
    internal class ExplorerManager
    {
        internal static readonly Dictionary<string, bool> InterfBtnMapping = new[]
        {
            new { Button = "TglButton7", NeedRestart = true },
            new { Button = "TglButton8", NeedRestart = true },
            new { Button = "TglButton16", NeedRestart = true },
            new { Button = "TglButton17", NeedRestart = true },
            new { Button = "TglButton21", NeedRestart = true },
            new { Button = "TglButton22", NeedRestart = true },
            new { Button = "TglButton26", NeedRestart = true },
            new { Button = "TglButton27", NeedRestart = true },
            new { Button = "TglButton30", NeedRestart = true },
            new { Button = "TglButton32", NeedRestart = true },
        }.ToDictionary(x => x.Button, x => x.NeedRestart);

        internal static readonly Dictionary<string, bool> AppImgMapping = new[]
{
            new { Package = "Widgets", NeedRestart = true },
            new { Package = "Edge", NeedRestart = true }
        }.ToDictionary(x => x.Package, x => x.NeedRestart);

        internal static void Restart(Process launchExplorer, Action action = null)
        {
            Task.Run(delegate
            {
                foreach (Process process in Process.GetProcesses())
                {
                    try
                    {
                        if (string.Compare(process.MainModule?.FileName, $@"{Environment.GetEnvironmentVariable("WINDIR")}\{"explorer.exe"}", StringComparison.OrdinalIgnoreCase) == 0 && Process.GetProcessesByName("explorer").Length != 0)
                        {
                            process.Kill();
                            action?.Invoke();
                            process.Start();
                        }
                    }
                    catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                    finally
                    {
                        if (Process.GetProcessesByName("explorer").Length == 0)
                        {
                            launchExplorer.StartInfo.FileName = $@"{Environment.GetEnvironmentVariable("WINDIR")}\explorer.exe";
                            launchExplorer.StartInfo.Arguments = "/factory,{EFD469A7-7E0A-4517-8B39-45873948DA31}";
                            launchExplorer.StartInfo.UseShellExecute = true;
                            launchExplorer.Start();
                        }
                    }
                }
            });
        }
    }
}
