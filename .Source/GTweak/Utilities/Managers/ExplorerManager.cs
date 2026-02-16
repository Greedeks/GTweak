using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GTweak.Utilities.Controls;

namespace GTweak.Utilities.Managers
{
    internal class ExplorerManager
    {
        internal static readonly Dictionary<string, bool> IntfMapping = new[]
        {
            new { Button = "TglButton7", NeedRestart = true },
            new { Button = "TglButton8", NeedRestart = true },
            new { Button = "TglButton16", NeedRestart = true },
            new { Button = "TglButton17", NeedRestart = true },
            new { Button = "TglButton21", NeedRestart = true },
            new { Button = "TglButton23", NeedRestart = true },
            new { Button = "TglButton27", NeedRestart = true },
            new { Button = "TglButton28", NeedRestart = true },
            new { Button = "TglButton31", NeedRestart = true },
            new { Button = "TglButton33", NeedRestart = true },
        }.ToDictionary(x => x.Button, x => x.NeedRestart);

        internal static readonly Dictionary<string, bool> PackageMapping = new[]
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
                        if (string.Compare(process.MainModule?.FileName, PathLocator.Executable.Explorer, StringComparison.OrdinalIgnoreCase) == 0 && Process.GetProcessesByName("explorer").Length != 0)
                        {
                            process.Kill();
                            action?.Invoke();
                            process.Start();
                        }
                    }
                    catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                    finally
                    {
                        if (Process.GetProcessesByName("explorer").Length == 0 && launchExplorer != null)
                        {
                            launchExplorer.StartInfo.FileName = PathLocator.Executable.Explorer;
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
