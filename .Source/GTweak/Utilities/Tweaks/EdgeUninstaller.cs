using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GTweak.Utilities.Controls;

namespace GTweak.Utilities.Tweaks
{
    internal class EdgeUninstaller
    {
        public static void RemoveEdge()
        {
            string basePath = @"C:\Program Files (x86)\Microsoft\Edge\Application";

            string setupPath;
            try
            {
                setupPath = Directory.GetDirectories(basePath)
                    .Select(dir => Path.Combine(dir, "Installer", "setup.exe"))
                    .FirstOrDefault(File.Exists);
            }
            catch (Exception ex)
            {
                ErrorLogging.LogDebug(ex);
                return;
            }

            if (setupPath == null)
            {
                return;
            }

            string fakeEdgePath = @"C:\Windows\SystemApps\Microsoft.MicrosoftEdge_8wekyb3d8bbwe\MicrosoftEdge.exe";

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fakeEdgePath));
                using (File.Create(fakeEdgePath)) { }
            }
            catch (Exception ex)
            {
                ErrorLogging.LogDebug(ex);
                return;
            }

            try
            {
                var process = new Process
                {
                    StartInfo =
                    {
                        FileName = setupPath,
                        Arguments = "--uninstall --system-level --force-uninstall --delete-profile",
                        UseShellExecute = false
                    }
                };

                process.Start();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                ErrorLogging.LogDebug(ex);
            }
        }
    }
}
