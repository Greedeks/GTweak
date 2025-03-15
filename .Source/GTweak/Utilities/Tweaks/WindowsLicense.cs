using GTweak.Utilities.Configuration;
using GTweak.Utilities.Control;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Helpers.Storage;
using GTweak.Windows;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GTweak.Utilities.Tweaks
{
    internal sealed class WindowsLicense : WinKeyStorage
    {
        class Metadata
        {
            public string Filename { get; set; }
            public string Content { get; set; }
        }

        internal static bool IsWindowsActivated = false;

        private static bool IsVersionWindows(string pattern, byte words) => new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled).Matches(SystemDiagnostics.WindowsClientVersion).Count == words;

        internal void LicenseStatus()
        {
            Parallel.Invoke(() =>
            {
                foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "SELECT LicenseStatus FROM SoftwareLicensingProduct WHERE ApplicationID = '55c92734-d682-4d71-983e-d6ec3f16059f' and LicenseStatus = 1").Get())
                    IsWindowsActivated = false;
            });
        }

        internal static async void StartActivation()
        {
            WaitingWindow waitingWindow = new WaitingWindow();

            string keyWindow = string.Empty, kmsArguments = string.Empty;

            foreach (var key in keysHWID)
            {
                if (IsVersionWindows(key.Key.pattern, key.Key.words))
                {
                    keyWindow = key.Value;
                    break;
                }
            }

            Process cmdProcess = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };

            async Task RunCommand(string arguments, int delay)
            {
                cmdProcess.StartInfo.Arguments = arguments;
                cmdProcess.Start();
                await Task.Delay(delay);
            }

            using (cmdProcess)
            {
                waitingWindow.Show();

                if (SystemDiagnostics.IsWindowsVersion[10])
                    await RunCommand("/c assoc .vbs=VBSFile", 500);

                await RunCommand($"/c slmgr.vbs //b /ipk {keyWindow}", 4000);

                if (!Directory.Exists(StoragePaths.FolderLocation))
                    Directory.CreateDirectory(StoragePaths.FolderLocation);
                File.WriteAllText(Path.Combine(StoragePaths.FolderLocation, "Tickets.json"), Encoding.UTF8.GetString(Properties.Resources.Tickets));

                try
                {
                    Metadata file = JsonConvert.DeserializeObject<List<Metadata>>(File.ReadAllText(Path.Combine(StoragePaths.FolderLocation, "Tickets.json")))?.FirstOrDefault(f => f.Filename.IndexOf(RegistryHelp.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\ProductOptions", "OSProductPfn", string.Empty), StringComparison.OrdinalIgnoreCase) >= 0);
                    XDocument xmlDoc = XDocument.Parse(file.Content);
                    xmlDoc.Save(Path.Combine(StoragePaths.SystemDisk, "ProgramData", "Microsoft", "Windows", "ClipSVC", "GenuineTicket", file.Filename));

                }
                catch (Exception ex) { Debug.WriteLine(ex.Message); }

                await Task.Delay(3000);
                CommandExecutor.RunCommand("clipup -v -o", true);
                await RunCommand("/c slmgr.vbs //b /ato", 3500);

                new WindowsLicense().LicenseStatus();

                await Task.Delay(2000);

                CommandExecutor.RunCommand($"/c timeout /t 10 && rd /s /q {StoragePaths.FolderLocation}");

                if (IsWindowsActivated)
                {
                    waitingWindow.Close();
                    new ViewNotification(300).Show("restart", "warn", "successactivate_notification");
                }
                else
                {
                    foreach (var key in keysKMS)
                    {
                        if (IsVersionWindows(key.Key.pattern, key.Key.words))
                        {
                            keyWindow = key.Value;
                            kmsArguments = key.Key.pattern == "Pro" ? @"/c slmgr.vbs //b /skms kms.digiboy.ir" : @"/c slmgr.vbs //b /skms kms.xspace.in";
                            break;
                        }
                    }

                    using (cmdProcess)
                    {
                        if (SystemDiagnostics.IsWindowsVersion[10])
                            await RunCommand("/c assoc .vbs=VBSFile", 500);

                        await RunCommand($"/c slmgr.vbs //b /ipk {keyWindow}", 4000);
                        await RunCommand(kmsArguments, 7000);
                        await RunCommand("/c slmgr.vbs //b /ato", 3500);

                        new WindowsLicense().LicenseStatus();

                        await Task.Delay(2000);

                        waitingWindow.Close();

                        if (IsWindowsActivated)
                            new ViewNotification(300).Show("restart", "warn", "successactivate_notification");
                        else
                            new ViewNotification(300).Show("", "warn", "notsuccessactivate_notification");
                    }
                }
            }
        }
    }
}
