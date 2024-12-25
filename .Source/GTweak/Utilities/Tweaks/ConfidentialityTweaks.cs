using GTweak.Properties;
using GTweak.Utilities.Configuration;
using GTweak.Utilities.Control;
using GTweak.Utilities.Helpers;
using GTweak.View;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GTweak.Utilities.Tweaks
{
    internal sealed class ConfidentialityTweaks : Firewall
    {
        private static readonly string[] SchedulerTasks = {
                @"Microsoft\Windows\Maintenance\WinSAT",
                @"Microsoft\Windows\Autochk\Proxy",
                @"Microsoft\Windows\Application Experience\Microsoft Compatibility Appraiser",
                @"Microsoft\Windows\Application Experience\ProgramDataUpdater",
                @"Microsoft\Windows\Application Experience\StartupAppTask",
                @"Microsoft\Windows\PI\Sqm-Tasks",
                @"Microsoft\Windows\NetTrace\GatherNetworkInfo",
                @"Microsoft\Windows\Customer Experience Improvement Program\Consolidator",
                @"Microsoft\Windows\Customer Experience Improvement Program\KernelCeipTask",
                @"Microsoft\Windows\Customer Experience Improvement Program\UsbCeip",
                @"Microsoft\Windows\DiskDiagnostic\Microsoft-Windows-DiskDiagnosticResolver",
                @"Microsoft\Windows\DiskDiagnostic\Microsoft-Windows-DiskDiagnosticDataCollector"};

        private static readonly string[] NvidiaTasks = {
                @"NvTmRepOnLogon_{B2FE1952-0186-46C3-BAEC-A80AA35AC5B8}",
                @"NvTmRep_{B2FE1952-0186-46C3-BAEC-A80AA35AC5B8}",
                @"NvTmMon_{B2FE1952-0186-46C3-BAEC-A80AA35AC5B8}" };

        private static readonly string[] TelemetryTasks = {
                @"Microsoft\Office\Office ClickToRun Service Monitor",
                @"Microsoft\Office\OfficeTelemetry\AgentFallBack2016",
                @"Microsoft\Office\OfficeTelemetry\OfficeTelemetryAgentLogOn2016",
                @"Microsoft\Office\OfficeTelemetryAgentFallBack2016",
                @"Microsoft\Office\OfficeTelemetryAgentLogOn2016",
                @"Microsoft\Office\OfficeTelemetryAgentFallBack",
                @"Microsoft\Office\OfficeTelemetryAgentLogOn",
                @"Microsoft\Office\Office 15 Subscription Heartbeat" };

        internal void ViewСonfidentiality(ConfidentialityView confidentialityV)
        {
            confidentialityV.TglButton1.StateNA =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PolicyManager\current\device\Bluetooth", "AllowAdvertising", "0");

            confidentialityV.TglButton2.StateNA =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Accessibility", "Enabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SettingSync\Groups\BrowserSettings", "Enabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Credentials", "Enabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Language", "Enabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Personalization", "Enabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Windows", "Enabled", "0");

            confidentialityV.TglButton3.StateNA =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\WMI\Autologger\Diagtrack-Listener", "Start", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Attachments", "SaveZoneInformation", "1");

            confidentialityV.TglButton4.StateNA = IsTaskEnabled(SchedulerTasks);

            confidentialityV.TglButton5.StateNA =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\AppCompat", "DisableInventory", "1");

            confidentialityV.TglButton6.StateNA =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\DataCollection", "AllowTelemetry", "0") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\AppCompat", "AITEnable", "0") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowDeviceNameInTelemetry", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackProgs", "0");

            confidentialityV.TglButton7.StateNA =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\TabletPC", "PreventHandwritingDataSharing", "1") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\HandwritingErrorReports", "PreventHandwritingErrorReports", "1") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Input\TIPC", "Enabled", "0");

            confidentialityV.TglButton8.StateNA =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\SQMClient\Windows", "CEIPEnable", "0");

            confidentialityV.TglButton9.StateNA = IsDefaultHosts();

            confidentialityV.TglButton10.StateNA =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors", "DisableLocation", "1") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors", "DisableLocationScripting", "1") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors", "DisableWindowsLocationProvider", "1");

            confidentialityV.TglButton11.StateNA =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Siuf\Rules", "NumberOfSIUFInPeriod", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Siuf\Rules", "PeriodInNanoSeconds", "0") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DataCollection", "DoNotShowFeedbackNotifications", "1");

            confidentialityV.TglButton12.StateNA =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Speech", "AllowSpeechModelUpdate", "0");

            confidentialityV.TglButton13.StateNA =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\CDPUserSvc", "Start", "4");

            confidentialityV.TglButton14.StateNA =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PolicyManager\current\device\System", "AllowExperimentation", "0");

            confidentialityV.TglButton15.StateNA =
                     RegistryHelp.CheckExists(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\DiagTrack") ||
                     RegistryHelp.CheckExists(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\dmwappushservice");

            confidentialityV.TglButton16.StateNA =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\diagnosticshub.standardcollector.service", "Start", "4");

            confidentialityV.TglButton17.StateNA = SystemDiagnostics.СonfigurationData["GPU"].ToLower().Contains("nvidia")
                && (RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\NvTelemetryContainer", "Start", "4") || IsTaskEnabled(NvidiaTasks));

            confidentialityV.TglButton18.StateNA =
                  RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\AppCompat", "DisableUAR", "1") ||
                  RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Personalization", "NoLockScreenCamera", "1");
        }

        internal bool IsDefaultHosts()
        {
            try
            {
                byte numberHostsRules = 0;

                using (StreamReader streamReader = new StreamReader(UsePath.Hosts))
                {
                    string hosts = streamReader.ReadToEnd();
                    numberHostsRules += (byte)(new string[] {
                        @"0.0.0.0 hk2sch130021829.wns.windows.com",
                        @"0.0.0.0 v10-win.vortex.data.microsoft.com.akadns.net",
                        @"0.0.0.0 watson.live.com",
                        @"0.0.0.0 cds843.lon.llnw.net",
                        @"0.0.0.0 db6sch102091602.wns.windows.com",
                        @"0.0.0.0 telemetry.microsoft.com",
                        @"0.0.0.0 bn3sch020010635.wns.windows.com",
                        @"0.0.0.0 api.cortana.ai"
                    }).Where(hostsrules => hosts.Contains(hostsrules)).Count();
                    streamReader.Close();
                }
                return numberHostsRules == 0;
            }
            catch { return false; }
        }

        internal static void UseСonfidentiality(string tweak, bool isChoose)
        {
            INIManager.TempWrite(INIManager.TempTweaksConf, tweak, isChoose);

            switch (tweak)
            {
                case "TglButton1":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\PolicyManager\current\device\Bluetooth", "AllowAdvertising", 0, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled");
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\PolicyManager\current\device\Bluetooth", "AllowAdvertising");
                    }
                    break;
                case "TglButton2":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\BrowserSettings", "Enabled", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Credentials", "Enabled", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Language", "Enabled", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Personalization", "Enabled", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Windows", "Enabled", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Accessibility", "Enabled", 0, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\BrowserSettings", "Enabled");
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Credentials", "Enabled");
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Language", "Enabled");
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"SOFTWARE\\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Personalization", "Enabled");
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"SOFTWARE\\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Windows", "Enabled");
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Accessibility", "Enabled");
                    }
                    break;
                case "TglButton3":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\WMI\Autologger\Diagtrack-Listener", "Start", isChoose ? 0 : 1, RegistryValueKind.DWord);

                    if (isChoose)
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Attachments", "SaveZoneInformation", 1, RegistryValueKind.DWord);
                    else
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Attachments", "SaveZoneInformation");

                    SetTaskState(TelemetryTasks, !isChoose);
                    break;
                case "TglButton4":
                    SetTaskState(SchedulerTasks, !isChoose);
                    break;
                case "TglButton5":
                    if (isChoose)
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppCompat", "DisableInventory", 1, RegistryValueKind.DWord);
                    else
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppCompat", "DisableInventory");
                    break;
                case "TglButton6":
                    RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\DataCollection", "AllowTelemetry", isChoose ? 0 : 1, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppCompat", "AITEnable", isChoose ? 0 : 1, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackProgs", isChoose ? 0 : 1, RegistryValueKind.DWord);

                    if (isChoose)
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowDeviceNameInTelemetry", 0, RegistryValueKind.DWord);
                    else
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowDeviceNameInTelemetry");
                    break;
                case "TglButton7":
                    RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\HandwritingErrorReports", "PreventHandwritingErrorReports", isChoose ? 1 : 0, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Input\TIPC", "Enabled", isChoose ? 0 : 1, RegistryValueKind.DWord);

                    if (isChoose)
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\TabletPC", "PreventHandwritingDataSharing", 1, RegistryValueKind.DWord);
                    else
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\TabletPC", "PreventHandwritingDataSharing");
                    break;
                case "TglButton8":
                    if (isChoose)
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\SQMClient\Windows", "CEIPEnable", 0, RegistryValueKind.DWord);
                    else
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\SQMClient");
                    break;
                case "TglButton9":
                    if (isChoose)
                    {
                        Task.Run(delegate
                        {
                            BlockSpyDomain(true);

                            try { File.Delete(UsePath.Hosts + @" (Default GTweak)"); } catch (Exception ex) { Debug.WriteLine(ex.Message); }

                            File.Move(UsePath.Hosts, UsePath.Hosts + " (Default GTweak)");
                            new UnarchiveManager(UsePath.Hosts, Resources.hosts);
                        });
                    }
                    else
                    {
                        Task.Run(delegate
                        {
                            try
                            {
                                BlockSpyDomain(false);
                                File.Copy(UsePath.Hosts + @" (Default GTweak)", UsePath.Hosts, true);
                                File.Delete(UsePath.Hosts + @" (Default GTweak)");
                            }
                            catch (Exception ex) { Debug.WriteLine(ex.Message); }
                        });
                    }
                    break;
                case "TglButton10":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors", "DisableLocation", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors", "DisableLocationScripting", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors", "DisableWindowsLocationProvider", 1, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors", "DisableLocation");
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors", "DisableLocationScripting");
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors", "DisableWindowsLocationProvider");
                    }
                    break;
                case "TglButton11":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Siuf\Rules", "NumberOfSIUFInPeriod", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Siuf\Rules", "PeriodInNanoSeconds", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "DoNotShowFeedbackNotifications", 1, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"SOFTWARE\Microsoft\Siuf\Rules", "NumberOfSIUFInPeriod");
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"SOFTWARE\Microsoft\Siuf\Rules", "PeriodInNanoSeconds");
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "DoNotShowFeedbackNotifications");
                    }
                    break;
                case "TglButton12":
                    if (isChoose)
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Speech", "AllowSpeechModelUpdate", 0, RegistryValueKind.DWord);
                    else
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Speech", "AllowSpeechModelUpdate");
                    break;
                case "TglButton13":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\CDPUserSvc", "Start", isChoose ? 4 : 2, RegistryValueKind.DWord);
                    break;
                case "TglButton14":
                    if (isChoose)
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\PolicyManager\current\device\System", "AllowExperimentation", 0, RegistryValueKind.DWord);
                    else
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\PolicyManager\current\device\System", "AllowExperimentation");
                    break;
                case "TglButton15":
                    if (isChoose)
                    {
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\DiagTrack");
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\dmwappushservice");
                    }
                    else
                    {
                        RegistryHelp.CreateFolder(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\DiagTrack");
                        RegistryHelp.CreateFolder(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\dmwappushservice");
                    }
                    break;
                case "TglButton16":
                    TrustedInstaller.CreateProcessAsTrustedInstaller(Control.Settings.PID, $@"cmd.exe /c reg add HKLM\SYSTEM\CurrentControlSet\Services\diagnosticshub.standardcollector.service /t REG_DWORD /v Start /d {(isChoose ? "4" : "3")} /f");
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\diagnosticshub.standardcollector.service", "Start", isChoose ? 4 : 3, RegistryValueKind.DWord, true);
                    break;
                case "TglButton17":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\NvTelemetryContainer", "Start", isChoose ? 4 : 2, RegistryValueKind.DWord);
                    SetTaskState(NvidiaTasks, !isChoose);
                    break;
                case "TglButton18":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppCompat", "DisableUAR", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Personalization", "NoLockScreenCamera", 1, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppCompat", "DisableUAR");
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Personalization", "NoLockScreenCamera");
                    }
                    break;
            }
        }
    }
}
