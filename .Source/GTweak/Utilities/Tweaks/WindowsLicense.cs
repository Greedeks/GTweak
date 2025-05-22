using GTweak.Utilities.Configuration;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Helpers.Storage;
using GTweak.Windows;
using Microsoft.Win32;
using System;
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

        private static bool IsVersionWindows(string pattern, byte words) => new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled).Matches(SystemDiagnostics.HardwareData.OSVersion).Count == words;

        internal static void LicenseStatus()
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

            new ViewNotification().Show("", "warn", "win_activate_notification");

            WaitingWindow waitingWindow = new WaitingWindow();
            waitingWindow.Show();

            try
            {
                if (SystemDiagnostics.IsWindowsVersion[10])
                    await CommandExecutor.InvokeRunCommand(@"/c assoc .vbs=VBSFile & ftype VBSFile=""%SystemRoot%\System32\WScript.exe"" ""%1"" %*""");

                await CommandExecutor.InvokeRunCommand($"/c slmgr.vbs //b /ipk {keyWinHWID}");

                CommandExecutor.RunCommand($@"/c del /f /q {StoragePaths.SystemDisk}ProgramData\Microsoft\Windows\ClipSVC\GenuineTicket\*.xml & del /f /q {StoragePaths.SystemDisk}ProgramData\Microsoft\Windows\ClipSVC\Install\Migration\*.xml");
                string originalGeo = RegistryHelp.GetValue(@"HKEY_CURRENT_USER\Control Panel\International\Geo", "Name", CultureInfo.InstalledUICulture.Name.Split('-')[1].ToUpperInvariant());
                RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\International\Geo", "Name", "US", RegistryValueKind.String);

                string svcRestartCmd = string.Join(" & ", new[] { "ClipSVC", "wlidsvc", "sppsvc", "KeyIso", "LicenseManager", "Winmgmt" }.Select(service => $"sc config {service} start= auto && sc start {service}"));
                await CommandExecutor.InvokeRunCommand($"/c {svcRestartCmd}");

                await Task.Delay(3000);

                XDocument xmlDoc = XDocument.Parse(Properties.Resources.Tickets);
                XElement foundTicket = xmlDoc.Descendants("Ticket").FirstOrDefault(t => t.Element("product") != null && t.Element("product").Value.IndexOf(RegistryHelp.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\ProductOptions", "OSProductPfn", string.Empty), StringComparison.OrdinalIgnoreCase) >= 0);
                foundTicket ??= xmlDoc.Descendants("Ticket").FirstOrDefault(t => t.Element("product") != null && t.Element("product").Value == "KMS");
                XDocument genuineXml = XDocument.Parse(foundTicket.Element("content")?.Value.Trim());
                genuineXml.Save(Path.Combine(StoragePaths.SystemDisk, "ProgramData", "Microsoft", "Windows", "ClipSVC", "GenuineTicket", "GenuineTicket.xml"));
                await Task.Delay(3000);

                await CommandExecutor.InvokeRunCommand("clipup -v -o", true);
                await CommandExecutor.InvokeRunCommand("/c slmgr.vbs //b /ato");

                RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\International\Geo", "Name", originalGeo, RegistryValueKind.String);

                LicenseStatus();

                await Task.Delay(2000);

                if (IsWindowsActivated)
                {
                    waitingWindow.Close();
                    new ViewNotification(300).Show("restart", "warn", "success_activate_notification");
                }
                else
                {
                    await CommandExecutor.InvokeRunCommand($"/c slmgr.vbs //b /ipk {keysKMS}");
                    await CommandExecutor.InvokeRunCommand("/c slmgr.vbs //b /skms kms.digiboy.ir");
                    await CommandExecutor.InvokeRunCommand("/c slmgr.vbs //b /ato");

                    LicenseStatus();

                    await Task.Delay(2000);

                    waitingWindow.Close();

                    new ViewNotification(300).Show(IsWindowsActivated ? "restart" : "", "warn", IsWindowsActivated ? "success_activate_notification" : "error_activate_notification");
                }
            }
            catch
            {
                waitingWindow.Close();
                new ViewNotification(300).Show("", "warn", "error_activate_notification");
            }
        }
    }
}
