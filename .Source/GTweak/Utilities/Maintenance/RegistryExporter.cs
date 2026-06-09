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
        internal readonly string[] RegistryPaths =
        {
            @"HKEY_CLASSES_ROOT\*\shellex",
            @"HKEY_CLASSES_ROOT\CLSID",
            @"HKEY_CLASSES_ROOT\Wow6432Node\CLSID",
            @"HKEY_CLASSES_ROOT\AppID",
            @"HKEY_CURRENT_USER\Software",
            @"HKEY_CURRENT_USER\System",
            @"HKEY_CURRENT_USER\Control Panel",
            @"HKEY_USERS\.DEFAULT\Control Panel",
            @"HKEY_USERS\S-1-5-19\Control Panel",
            @"HKEY_USERS\S-1-5-20\Control Panel",
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft",
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Policies",
            @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet",
            @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft",
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Classes",
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Clients"
        };

        private static bool RegistryKeyExists(string fullPath)
        {
            int index = fullPath.IndexOf('\\');
            if (index < 0)
            {
                return false;
            }

            string hive = fullPath.Substring(0, index);
            string subKey = fullPath.Substring(index + 1);

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
                using RegistryKey key = baseKey.OpenSubKey(subKey);
                return key != null;
            }
            catch { return false; }
        }

        internal void Export(string fileName)
        {
            string tempDir = PathLocator.Folders.Workspace;
            Directory.CreateDirectory(tempDir);

            List<string> tempFiles = new List<string>();
            try
            {
                foreach (string path in RegistryPaths)
                {
                    if (!RegistryKeyExists(path))
                    {
                        continue;
                    }

                    string tempFile = Path.Combine(tempDir, Path.GetRandomFileName() + ".reg");
                    tempFiles.Add(tempFile);

                    using Process proc = Process.Start(new ProcessStartInfo("reg.exe", $"EXPORT \"{path}\" \"{tempFile}\" /y")
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false
                    });
                    proc.WaitForExit();
                }

                using StreamWriter writer = new StreamWriter(fileName, false, Encoding.Unicode);
                foreach (string file in tempFiles)
                {
                    writer.WriteLine(File.ReadAllText(file, Encoding.Unicode));
                }
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