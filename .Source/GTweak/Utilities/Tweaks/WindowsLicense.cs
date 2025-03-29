using GTweak.Utilities.Configuration;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Helpers.Storage;
using GTweak.Windows;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Globalization;
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

        internal static async Task StartActivation()
        {
            string keyWinHWID = keysHWID.FirstOrDefault(k => IsVersionWindows(k.Key.pattern, k.Key.words)).Value ?? string.Empty;
            string keyWinKMS = keysKMS.FirstOrDefault(k => IsVersionWindows(k.Key.pattern, k.Key.words)).Value ?? string.Empty;

            if (string.IsNullOrEmpty(keyWinHWID) && string.IsNullOrEmpty(keyWinKMS))
            {
                new ViewNotification(300).Show("", "warn", "keynotfound_notification");
                return;
            }

            new ViewNotification().Show("", "warn", "activatewin_notification");

            static async Task RunCommandAsync(string arguments, int delay)
            {
                using Process cmdProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = arguments,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    }
                };
                cmdProcess.Start();
                await Task.Delay(delay);
            }

            WaitingWindow waitingWindow = new WaitingWindow();
            waitingWindow.Show();

            try
            {
                if (SystemDiagnostics.IsWindowsVersion[10])
                    await RunCommandAsync("/c assoc .vbs=VBSFile", 500);

                await RunCommandAsync($"/c slmgr.vbs //b /ipk {keyWinHWID}", 4000);

                CommandExecutor.RunCommand($@"/c del /f /q {StoragePaths.SystemDisk}ProgramData\Microsoft\Windows\ClipSVC\GenuineTicket\*.xml & del /f /q {StoragePaths.SystemDisk}ProgramData\Microsoft\Windows\ClipSVC\Install\Migration\*.xml");
                string originalGeo = RegistryHelp.GetValue(@"HKEY_CURRENT_USER\Control Panel\International\Geo", "Name", CultureInfo.InstalledUICulture.Name.Split('-')[1].ToUpper());
                RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\International\Geo", "Name", "US", RegistryValueKind.String);
                foreach (string service in new string[] { "ClipSVC", "wlidsvc", "sppsvc", "KeyIso", "LicenseManager", "Winmgmt" })
                    CommandExecutor.RunCommand($"sc config {service} start= auto && sc start {service}");

                await Task.Delay(3000);

                XDocument xmlDoc = XDocument.Parse(Properties.Resources.Tickets);
                XElement foundTicket = xmlDoc.Descendants("Ticket").FirstOrDefault(t => t.Element("product") != null && t.Element("product").Value.IndexOf(RegistryHelp.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\ProductOptions", "OSProductPfn", string.Empty), StringComparison.OrdinalIgnoreCase) >= 0);
                foundTicket ??= xmlDoc.Descendants("Ticket").FirstOrDefault(t => t.Element("product") != null && t.Element("product").Value == "KMS");
                XDocument genuineXml = XDocument.Parse(foundTicket.Element("content")?.Value.Trim());
                genuineXml.Save(Path.Combine(StoragePaths.SystemDisk, "ProgramData", "Microsoft", "Windows", "ClipSVC", "GenuineTicket", "GenuineTicket.xml"));
                await Task.Delay(3000);
                CommandExecutor.RunCommand("clipup -v -o", true);

                await Task.Delay(1000);
                await RunCommandAsync("/c slmgr.vbs //b /ato", 3500);

                RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\International\Geo", "Name", originalGeo, RegistryValueKind.String);

                new WindowsLicense().LicenseStatus();

                await Task.Delay(2000);

                if (IsWindowsActivated)
                {
                    waitingWindow.Close();
                    new ViewNotification(300).Show("restart", "warn", "successactivate_notification");
                }
                else
                {
                    await RunCommandAsync($"/c slmgr.vbs //b /ipk {keysKMS}", 4000);
                    await RunCommandAsync("/c slmgr.vbs //b /skms kms.digiboy.ir", 7000);
                    await RunCommandAsync("/c slmgr.vbs //b /ato", 3500);

                    new WindowsLicense().LicenseStatus();

                    await Task.Delay(2000);

                    waitingWindow.Close();

                    new ViewNotification(300).Show(IsWindowsActivated ? "restart" : "", "warn", IsWindowsActivated ? "successactivate_notification" : "notsuccessactivate_notification");
                }
            }
            catch
            {
                waitingWindow.Close();
                new ViewNotification(300).Show("", "warn", "notsuccessactivate_notification");
            }
        }
    }
}
