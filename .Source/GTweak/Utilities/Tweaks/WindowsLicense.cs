using GTweak.Utilities.Configuration;
using GTweak.Utilities.Control;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Helpers.Storage;
using GTweak.Windows;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GTweak.Utilities.Tweaks
{
    internal sealed class WindowsLicense : WinKeyStorage
    {
        private sealed class Metadata
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
                    IsWindowsActivated = (uint)managementObj["LicenseStatus"] == 1;
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

                new UnarchiveManager(StoragePaths.FolderLocation + @"\Tickets.json", Properties.Resources.Tickets);

                try
                {
                    string jsonPath = Path.Combine(StoragePaths.FolderLocation, "Tickets.json");
                    string jsonContent = File.Exists(jsonPath) ? File.ReadAllText(jsonPath) : "[]";
                    List<Metadata> metadataList = JsonConvert.DeserializeObject<List<Metadata>>(jsonContent) ?? new List<Metadata>();
                    Metadata file = metadataList.FirstOrDefault(f => f.Filename.IndexOf(RegistryHelp.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\ProductOptions", "OSProductPfn", string.Empty), StringComparison.OrdinalIgnoreCase) >= 0);

                    if (file != null)
                    {
                        XDocument xmlDoc = XDocument.Parse(file.Content);
                        xmlDoc.Save(Path.Combine(StoragePaths.SystemDisk, "ProgramData", "Microsoft", "Windows", "ClipSVC", "GenuineTicket", file.Filename));
                        await Task.Delay(3000);
                        CommandExecutor.RunCommand("clipup -v -o", true);
                    }
                    else
                    {
                        string path = Path.Combine(StoragePaths.FolderLocation, "gatherosstatemodified.exe");
                        new UnarchiveManager(path, Properties.Resources.gatherosstatemodified);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers", path, "~ RUNASADMIN WINXPSP3", RegistryValueKind.String);
                        DateTime userTime = DateTime.Now;
                        CommandExecutor.RunCommand($"Set-Date -Date '{new DateTime(2022, 10, 11, 12, 0, 0):yyyy-MM-dd HH:mm:ss}'", true);
                        await Task.Delay(2000);
                        CommandExecutor.RunCommand($@"{path} /c Pfn={RegistryHelp.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\ProductOptions", "OSProductPfn", string.Empty)};PKeyIID=465145217131314304264339481117862266242033457260311819664735280", true);
                        await Task.Delay(1000);
                        CommandExecutor.RunCommand($"Set-Date -Date '{userTime:yyyy-MM-dd HH:mm:ss}'", true);
                        CommandExecutor.RunCommand($"clipup -v -o -altto {StoragePaths.FolderLocation}", true);
                    }
                }
                catch (Exception ex) { Debug.WriteLine(ex.Message); }

                await Task.Delay(1000);
                await RunCommand("/c slmgr.vbs //b /ato", 3500);
                CommandExecutor.RunCommand($"/c timeout /t 10 && rd /s /q {StoragePaths.FolderLocation}");

                new WindowsLicense().LicenseStatus();

                await Task.Delay(2000);

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
