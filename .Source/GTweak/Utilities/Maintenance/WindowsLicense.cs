using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using GTweak.Utilities.Configuration;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Managers;
using GTweak.Utilities.Storage;
using GTweak.Windows;
using Microsoft.Win32;

namespace GTweak.Utilities.Maintenance
{
    internal sealed class WindowsLicense : WinKeyStorage
    {
        internal static bool IsWindowsActivated = false;

        private static bool IsVersionWindows(string pattern, byte words) => new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled).Matches(HardwareData.OS.Name).Count == words;

        internal static async void LicenseStatus()
        {
            try
            {
                foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "SELECT LicenseStatus FROM SoftwareLicensingProduct WHERE ApplicationID = '55c92734-d682-4d71-983e-d6ec3f16059f' and LicenseStatus = 1").Get())
                {
                    IsWindowsActivated = (uint)managementObj["LicenseStatus"] == 1;
                }
            }
            catch (COMException)
            {
                try
                {
                    string output = await CommandExecutor.GetCommandOutput("cscript //B \"$env:windir\\system32\\slmgr.vbs\" /ato; $code = $LASTEXITCODE; ($code -eq 0 -or $code -eq -2147024773)", true);
                    bool.TryParse(output, out IsWindowsActivated);
                }
                catch (Exception ex) { ErrorLogging.LogDebug(ex); }
            }
            catch (Exception ex) { ErrorLogging.LogDebug(ex); }
        }

        internal static async Task StartActivation()
        {
            string keyWinHWID = keysHWID.FirstOrDefault(k => IsVersionWindows(k.Key.pattern, k.Key.words)).Value ?? string.Empty;
            string keyWinKMS = keysKMS.FirstOrDefault(k => IsVersionWindows(k.Key.pattern, k.Key.words)).Value ?? string.Empty;

            if (string.IsNullOrEmpty(keyWinHWID) && string.IsNullOrEmpty(keyWinKMS))
            {
                NotificationManager.Show("warn", "keynotfound_noty").WithDelay(300).Perform();
                return;
            }

            NotificationManager.Show("warn", "win_activate_noty").Perform();

            OverlayWindow overlayWindow = new OverlayWindow();
            overlayWindow.Show();

            try
            {
                if (HardwareData.OS.IsWin10)
                {
                    await CommandExecutor.InvokeRunCommand(@"/c assoc .vbs=VBSFile & ftype VBSFile=""%SystemRoot%\System32\WScript.exe"" ""%1"" %*""");
                }

                await CommandExecutor.InvokeRunCommand($"/c slmgr.vbs //b /ipk {keyWinHWID}");

                CommandExecutor.RunCommand($@"/c del /f /q {PathLocator.Folders.SystemDrive}ProgramData\Microsoft\Windows\ClipSVC\GenuineTicket\*.xml & del /f /q {PathLocator.Folders.SystemDrive}ProgramData\Microsoft\Windows\ClipSVC\Install\Migration\*.xml");
                string originalGeo = RegistryHelp.GetValue(@"HKEY_CURRENT_USER\Control Panel\International\Geo", "Name", CultureInfo.InstalledUICulture.Name.Split('-')[1].ToUpperInvariant());
                RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\International\Geo", "Name", "US", RegistryValueKind.String);

                string svcRestartCmd = string.Join(" & ", new[] { "ClipSVC", "wlidsvc", "sppsvc", "KeyIso", "LicenseManager", "Winmgmt" }.Select(service => $"sc config {service} start= auto && sc start {service}"));
                await CommandExecutor.InvokeRunCommand($"/c {svcRestartCmd}");

                await Task.Delay(3000);

                XDocument xmlDoc = XDocument.Parse(Properties.Resources.Tickets);
                XElement foundTicket = xmlDoc.Descendants("Ticket").FirstOrDefault(t => t.Element("product") != null && t.Element("product").Value.IndexOf(RegistryHelp.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\ProductOptions", "OSProductPfn", string.Empty), StringComparison.OrdinalIgnoreCase) >= 0);
                foundTicket ??= xmlDoc.Descendants("Ticket").FirstOrDefault(t => t.Element("product") != null && t.Element("product").Value == "KMS");
                XDocument genuineXml = XDocument.Parse(foundTicket.Element("content")?.Value.Trim());
                genuineXml.Save(Path.Combine(PathLocator.Folders.SystemDrive, "ProgramData", "Microsoft", "Windows", "ClipSVC", "GenuineTicket", "GenuineTicket.xml"));
                await Task.Delay(3000);

                await CommandExecutor.InvokeRunCommand("clipup -v -o", true);
                await CommandExecutor.InvokeRunCommand("/c slmgr.vbs //b /ato");

                RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\International\Geo", "Name", originalGeo, RegistryValueKind.String);

                LicenseStatus();

                await Task.Delay(2000);

                if (IsWindowsActivated)
                {
                    overlayWindow.Close();
                    NotificationManager.Show("warn", "success_activate_noty").WithDelay(300).Restart();
                }
                else
                {
                    await CommandExecutor.InvokeRunCommand($"/c slmgr.vbs //b /ipk {keysKMS}");
                    await CommandExecutor.InvokeRunCommand("/c slmgr.vbs //b /skms kms.digiboy.ir");
                    await CommandExecutor.InvokeRunCommand("/c slmgr.vbs //b /ato");

                    LicenseStatus();

                    await Task.Delay(2000);

                    overlayWindow.Close();

                    NotificationManager.Show("warn", IsWindowsActivated ? "success_activate_noty" : "error_activate_noty").WithDelay(300).Perform(IsWindowsActivated ? NotificationManager.NoticeAction.Restart : default);
                }
            }
            catch
            {
                overlayWindow.Close();
                NotificationManager.Show("warn", "error_activate_noty").WithDelay(300).Perform();
            }
        }
    }
}
