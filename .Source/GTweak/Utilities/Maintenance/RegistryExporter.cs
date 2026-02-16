using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Managers;
using Microsoft.Win32;

namespace GTweak.Utilities.Maintenance
{
    internal sealed class RegistryExporter
    {
        internal string[] RegistryPaths = {
            @"HKEY_CLASSES_ROOT\*\shellex",
            @"HKEY_CLASSES_ROOT\CLSID",
            @"HKEY_CLASSES_ROOT\Wow6432Node\CLSID",
            @"HKEY_CLASSES_ROOT\AppID\MicrosoftEdgeUpdate.exe",
            @"HKEY_CURRENT_USER\Software\Microsoft",
            @"HKEY_CURRENT_USER\Control Panel",
            @"HKEY_CURRENT_USER\Software\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion",
            @"HKEY_CURRENT_USER\Software\Microsoft\EdgeUpdate",
            @"HKEY_CURRENT_USER\Software\Microsoft\EdgeWebView",
            @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run",
            @"HKEY_USERS\.DEFAULT\Control Panel",
            @"HKEY_USERS\S-1-5-19\Control Panel",
            @"HKEY_USERS\S-1-5-20\Control Panel",
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PolicyManager\current\device",
            @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control",
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows",
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Edge",
            @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services",
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion",
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Internet Explorer",
            @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Edge",
            @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Microsoft Edge",
            @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Microsoft Edge Update",
            @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\edgeupdate",
            @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\edgeupdatem",
            @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate",
            @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\EdgeWebView",
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\EdgeWebView",
            @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Microsoft EdgeWebView",
            @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\MicrosoftEdgeElevationService",
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\MSEdgeHTM",
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Clients\StartMenuInternet\Microsoft Edge"
        };

        private bool RegistryKeyExists(string fullPath)
        {
            int index = fullPath.IndexOf('\\');
            if (index < 0)
            {
                return false;
            }

            string hive = !string.IsNullOrEmpty(fullPath) && index >= 0 && index <= fullPath.Length ? fullPath.Substring(0, index) : string.Empty;
            string subKey = (fullPath != null && index + 1 < fullPath.Length) ? fullPath.Substring(index + 1) : string.Empty;

            RegistryKey baseKey = hive switch
            {
                "HKEY_CLASSES_ROOT" => Registry.ClassesRoot,
                "HKEY_CURRENT_USER" => Registry.CurrentUser,
                "HKEY_LOCAL_MACHINE" => Registry.LocalMachine,
                "HKEY_USERS" => Registry.Users,
                "HKEY_CURRENT_CONFIG" => Registry.CurrentConfig,
                _ => null
            };

            if (baseKey == null)
            {
                return false;
            }

            try
            {
                using var key = baseKey.OpenSubKey(subKey);
                return key != null;
            }
            catch { return false; }
        }

        internal void Export(string fileName)
        {
            string finalRegFile = fileName;
            string tempDir = PathLocator.Folders.Workspace;
            Directory.CreateDirectory(tempDir);

            List<string> tempFiles = new List<string>();
            try
            {
                foreach (var path in RegistryPaths)
                {
                    if (!RegistryKeyExists(path))
                    {
                        continue;
                    }

                    string tempFile = Path.Combine(tempDir, Path.GetRandomFileName() + ".reg");
                    tempFiles.Add(tempFile);

                    ProcessStartInfo psi = new ProcessStartInfo("reg.exe", $"EXPORT \"{path}\" \"{tempFile}\" /y")
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    Process proc = Process.Start(psi);
                    proc.WaitForExit();
                }

                StringBuilder sb = new StringBuilder();
                foreach (string file in tempFiles)
                {
                    sb.AppendLine(File.ReadAllText(file, Encoding.Unicode));
                }

                File.WriteAllText(finalRegFile, sb.ToString(), Encoding.Unicode);
            }
            catch { NotificationManager.Show("warn", "error_reg_exporter_noty").Perform(); }
            finally
            {
                foreach (string file in tempFiles)
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                }

                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir);
                }

                NotificationManager.Show("info", "success_reg_exporter_noty").WithDelay(500).Perform();
            }
        }
    }
}