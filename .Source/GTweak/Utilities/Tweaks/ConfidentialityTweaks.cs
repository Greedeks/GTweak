using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GTweak.Properties;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Managers;
using Microsoft.Win32;

namespace GTweak.Utilities.Tweaks
{
    internal sealed class ConfidentialityTweaks : FirewallManager
    {
        internal static Dictionary<string, object> ControlStates = new Dictionary<string, object>();
        private readonly ControlWriterManager _сontrolWriter = new ControlWriterManager(ControlStates);

        internal void AnalyzeAndUpdate()
        {
            _сontrolWriter.Button[1] =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PolicyManager\current\device\Bluetooth", "AllowAdvertising", "0");

            _сontrolWriter.Button[2] =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Accessibility", "Enabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SettingSync\Groups\BrowserSettings", "Enabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Credentials", "Enabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Language", "Enabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Personalization", "Enabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Windows", "Enabled", "0");

            _сontrolWriter.Button[3] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\WMI\Autologger\Diagtrack-Listener", "Start", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Attachments", "SaveZoneInformation", "1") || IsTaskEnabled(telemetryTasks);

            _сontrolWriter.Button[4] = IsTaskEnabled(dataCollectTasks);

            _сontrolWriter.Button[5] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\AppCompat", "DisableInventory", "1");

            _сontrolWriter.Button[6] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\DataCollection", "AllowTelemetry", "0") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\AppCompat", "AITEnable", "0") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowDeviceNameInTelemetry", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackProgs", "0");

            _сontrolWriter.Button[7] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\TabletPC", "PreventHandwritingDataSharing", "1") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\HandwritingErrorReports", "PreventHandwritingErrorReports", "1") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Input\TIPC", "Enabled", "0");

            _сontrolWriter.Button[8] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\SQMClient\Windows", "CEIPEnable", "0");

            _сontrolWriter.Button[9] = IsDefaultHosts();

            _сontrolWriter.Button[10] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors", "DisableLocation", "1") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors", "DisableLocationScripting", "1") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors", "DisableWindowsLocationProvider", "1");

            _сontrolWriter.Button[11] =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Siuf\Rules", "NumberOfSIUFInPeriod", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Siuf\Rules", "PeriodInNanoSeconds", "0") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DataCollection", "DoNotShowFeedbackNotifications", "1");

            _сontrolWriter.Button[12] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Speech", "AllowSpeechModelUpdate", "0");

            _сontrolWriter.Button[13] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\CDPUserSvc", "Start", "4");

            _сontrolWriter.Button[14] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PolicyManager\current\device\System", "AllowExperimentation", "0");

            _сontrolWriter.Button[15] =
                     RegistryHelp.KeyExists(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\DiagTrack") ||
                     RegistryHelp.KeyExists(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\dmwappushservice") ||
                     RegistryHelp.KeyExists(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\diagsvc");

            _сontrolWriter.Button[16] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\diagnosticshub.standardcollector.service", "Start", "4");

            _сontrolWriter.Button[17] =
                (RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\NvTelemetryContainer", "Start", "4") || IsTaskEnabled(nvidiaTasks));

            _сontrolWriter.Button[18] =
                  RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\AppCompat", "DisableUAR", "1") ||
                  RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Personalization", "NoLockScreenCamera", "1");
        }

        internal bool IsDefaultHosts()
        {
            try
            {
                int numberRows = File.ReadAllText(PathLocator.Files.Hosts).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Count(line => line.StartsWith("0.0.0.0"));
                return numberRows < 673;
            }
            catch { return false; }
        }

        internal void ApplyTweaks(string tweak, bool isDisabled)
        {
            INIManager.TempWrite(INIManager.TempTweaksConf, tweak, isDisabled);

            switch (tweak)
            {
                case "TglButton1":
                    if (isDisabled)
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
                    if (isDisabled)
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\BrowserSettings", "Enabled", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Credentials", "Enabled", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Language", "Enabled", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Personalization", "Enabled", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Windows", "Enabled", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Accessibility", "Enabled", 0, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\BrowserSettings", "Enabled");
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Credentials", "Enabled");
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Language", "Enabled");
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Personalization", "Enabled");
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Windows", "Enabled");
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Accessibility", "Enabled");
                    }
                    break;
                case "TglButton3":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\WMI\Autologger\Diagtrack-Listener", "Start", isDisabled ? 0 : 1, RegistryValueKind.DWord);

                    if (isDisabled)
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Attachments", "SaveZoneInformation", 1, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Attachments", "SaveZoneInformation");
                    }

                    SetTaskState(!isDisabled, telemetryTasks);
                    break;
                case "TglButton4":
                    SetTaskState(!isDisabled, dataCollectTasks);
                    break;
                case "TglButton5":
                    if (isDisabled)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppCompat", "DisableInventory", 1, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppCompat", "DisableInventory");
                    }

                    break;
                case "TglButton6":
                    RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\DataCollection", "AllowTelemetry", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppCompat", "AITEnable", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackProgs", isDisabled ? 0 : 1, RegistryValueKind.DWord);

                    if (isDisabled)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowDeviceNameInTelemetry", 0, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowDeviceNameInTelemetry");
                    }

                    break;
                case "TglButton7":
                    RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\HandwritingErrorReports", "PreventHandwritingErrorReports", isDisabled ? 1 : 0, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Input\TIPC", "Enabled", isDisabled ? 0 : 1, RegistryValueKind.DWord);

                    if (isDisabled)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\TabletPC", "PreventHandwritingDataSharing", 1, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\TabletPC", "PreventHandwritingDataSharing");
                    }

                    break;
                case "TglButton8":
                    if (isDisabled)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\SQMClient\Windows", "CEIPEnable", 0, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\SQMClient");
                    }

                    break;
                case "TglButton9":
                    BlockSpyDomain(isDisabled);
                    Task.Run(delegate
                    {
                        string backupFile = PathLocator.Files.Hosts + @" (Default GTweak)";
                        try
                        {
                            if (isDisabled)
                            {
                                if (File.Exists(backupFile))
                                {
                                    File.Delete(backupFile);
                                }

                                File.Move(PathLocator.Files.Hosts, backupFile);
                                ArchiveManager.Unarchive(PathLocator.Files.Hosts, Resources.hosts);
                            }
                            else
                            {
                                File.Copy(backupFile, PathLocator.Files.Hosts, true);

                                if (File.Exists(backupFile))
                                {
                                    File.Delete(backupFile);
                                }
                                else
                                {
                                    File.WriteAllText(PathLocator.Files.Hosts, string.Empty);
                                }
                            }
                        }
                        catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                    });
                    break;
                case "TglButton10":
                    if (isDisabled)
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
                    if (isDisabled)
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
                    if (isDisabled)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Speech", "AllowSpeechModelUpdate", 0, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Speech", "AllowSpeechModelUpdate");
                    }

                    break;
                case "TglButton13":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\CDPUserSvc", "Start", isDisabled ? 4 : 2, RegistryValueKind.DWord);
                    break;
                case "TglButton14":
                    if (isDisabled)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\PolicyManager\current\device\System", "AllowExperimentation", 0, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\PolicyManager\current\device\System", "AllowExperimentation");
                    }

                    break;
                case "TglButton15":
                    (string diagTrack, string dmwappushservice, string diagsvc) =
                    (
                        @"SYSTEM\CurrentControlSet\Services\DiagTrack",
                        @"SYSTEM\CurrentControlSet\Services\dmwappushservice",
                        @"SYSTEM\CurrentControlSet\Services\diagsvc"
                    );

                    if (isDisabled)
                    {
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, diagTrack);
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, dmwappushservice);
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, diagsvc);
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.LocalMachine, diagTrack, "DependOnService", new[] { "RpcSs" }, RegistryValueKind.MultiString);
                        RegistryHelp.Write(Registry.LocalMachine, diagTrack, "Description", @"@%SystemRoot%\system32\diagtrack.dll,-3002", RegistryValueKind.String);
                        RegistryHelp.Write(Registry.LocalMachine, diagTrack, "DisplayName", @"@%SystemRoot%\system32\diagtrack.dll,-3001", RegistryValueKind.String);
                        RegistryHelp.Write(Registry.LocalMachine, diagTrack, "ErrorControl", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, diagTrack, "FailureActions", Array.ConvertAll("80,51,01,00,00,00,00,00,00,00,00,00,03,00,00,00,14,00,00,00,01,00,00,00,30,75,00,00,01,00,00,00,30,75,00,00,00,00,00,00,00,00,00,00".Split(','), s => Convert.ToByte(s, 16)), RegistryValueKind.Binary);
                        RegistryHelp.Write(Registry.LocalMachine, diagTrack, "ImagePath", @"%SystemRoot%\System32\svchost.exe -k utcsvc -p", RegistryValueKind.ExpandString);
                        RegistryHelp.Write(Registry.LocalMachine, diagTrack, "ObjectName", "LocalSystem", RegistryValueKind.String);
                        RegistryHelp.Write(Registry.LocalMachine, diagTrack, "RequiredPrivileges", new[] { "SeChangeNotifyPrivilege", "SeCreateGlobalPrivilege", "SeAssignPrimaryTokenPrivilege", "SeImpersonatePrivilege", "SeSystemProfilePrivilege", "SeTcbPrivilege", "SeDebugPrivilege", "SeSecurityPrivilege" }, RegistryValueKind.MultiString);
                        RegistryHelp.Write(Registry.LocalMachine, diagTrack, "ServiceSidType", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, diagTrack, "Start", 2, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, diagTrack, "Type", 16, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, $@"{diagTrack}\Parameters", "ServiceDll", @"%SystemRoot%\system32\diagtrack.dll", RegistryValueKind.ExpandString);
                        RegistryHelp.Write(Registry.LocalMachine, $@"{diagTrack}\Parameters", "ServiceDllUnloadOnStop", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, $@"{diagTrack}\Parameters", "ServiceMain", "ServiceMain", RegistryValueKind.String);

                        RegistryHelp.Write(Registry.LocalMachine, dmwappushservice, "DelayedAutoStart", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, dmwappushservice, "DependOnService", new[] { "rpcss" }, RegistryValueKind.MultiString);
                        RegistryHelp.Write(Registry.LocalMachine, dmwappushservice, "Description", @"@%SystemRoot%\system32\dmwappushsvc.dll,-201", RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, dmwappushservice, "DisplayName", @"@%SystemRoot%\system32\dmwappushsvc.dll,-200", RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, dmwappushservice, "ErrorControl", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, dmwappushservice, "FailureActions", Array.ConvertAll("80,51,01,00,00,00,00,00,00,00,00,00,04,00,00,00,14,00,00,00,01,00,00,00,10,27,00,00,01,00,00,00,10,27,00,00,01,00,00,00,10,27,00,00,00,00,00,00,10,27,00,00".Split(','), s => Convert.ToByte(s, 16)), RegistryValueKind.Binary);
                        RegistryHelp.Write(Registry.LocalMachine, dmwappushservice, "ImagePath", @"%SystemRoot%\system32\svchost.exe -k netsvcs -p", RegistryValueKind.ExpandString);
                        RegistryHelp.Write(Registry.LocalMachine, dmwappushservice, "ObjectName", "LocalSystem", RegistryValueKind.String);
                        RegistryHelp.Write(Registry.LocalMachine, dmwappushservice, "ServiceSidType", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, dmwappushservice, "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, dmwappushservice, "Type", 20, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, dmwappushservice, "StateFlags", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, $@"{dmwappushservice}\Parameters", "IdleTimeout(sec)", 120, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, $@"{dmwappushservice}\Parameters", "ServiceDll", @"%SystemRoot%\system32\dmwappushsvc.dll", RegistryValueKind.ExpandString);
                        RegistryHelp.Write(Registry.LocalMachine, $@"{dmwappushservice}\Parameters", "ServiceDllUnloadOnStop", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, $@"{dmwappushservice}\Parameters", "ServiceMain", "ServiceMain", RegistryValueKind.String);
                        RegistryHelp.Write(Registry.LocalMachine, $@"{dmwappushservice}\TriggerInfo\0", "Action", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, $@"{dmwappushservice}\TriggerInfo\0", "Data0", Array.ConvertAll("37,00,39,00,35,00,42,00,36,00,42,00,46,00,39,00,2d,00,39,00,37,00,42,00,36,00,2d,00,34,00,46,00,38,00,39,00,2d,00,42,00,44,00,38,00,44,00,2d,00,32,00,46,00,34,00,32,00,42,00,42,00,42,00,45,00,39,00,39,00,36,00,45,00,00,00".Split(','), s => Convert.ToByte(s, 16)), RegistryValueKind.Binary);
                        RegistryHelp.Write(Registry.LocalMachine, $@"{dmwappushservice}\TriggerInfo\0", "DataType0", 2, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, $@"{dmwappushservice}\TriggerInfo\0", "GUID", Array.ConvertAll("67,d1,90,bc,70,94,39,41,a9,ba,be,0b,bb,f5,b7,4d".Split(','), s => Convert.ToByte(s, 16)), RegistryValueKind.Binary);
                        RegistryHelp.Write(Registry.LocalMachine, $@"{dmwappushservice}\TriggerInfo\0", "Type", 6, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, $@"{dmwappushservice}\TriggerInfo\1", "Action", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, $@"{dmwappushservice}\TriggerInfo\1", "Data0", Array.ConvertAll("75,90,bc,a3,28,00,92,13".Split(','), s => Convert.ToByte(s, 16)), RegistryValueKind.Binary);
                        RegistryHelp.Write(Registry.LocalMachine, $@"{dmwappushservice}\TriggerInfo\1", "DataType0", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, $@"{dmwappushservice}\TriggerInfo\1", "GUID", Array.ConvertAll("16,28,7a,2d,5e,0c,fc,45,9c,e7,57,0e,5e,cd,e9,c9".Split(','), s => Convert.ToByte(s, 16)), RegistryValueKind.Binary);
                        RegistryHelp.Write(Registry.LocalMachine, $@"{dmwappushservice}\TriggerInfo\1", "Type", 7, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, $@"{dmwappushservice}\Security", "Security", Array.ConvertAll("01,00,04,80,b0,00,00,00,bc,00,00,00,00,00,00,00,14,00,00,00,02,00,9c,00,07,00,00,00,00,00,14,00,8d,01,02,00,01,01,00,00,00,00,00,05,04,00,00,00,00,00,14,00,8d,01,02,00,01,01,00,00,00,00,00,05,06,00,00,00,00,00,14,00,ff,01,0f,00,01,01,00,00,00,00,00,05,12,00,00,00,00,00,18,00,ff,01,0f,00,01,02,00,00,00,00,00,05,20,00,00,00,20,02,00,00,00,00,18,00,14,00,00,00,01,02,00,00,00,00,00,0f,02,00,00,00,01,00,00,00,00,00,14,00,14,00,00,00,01,01,00,00,00,00,00,05,04,00,00,00,00,00,14,00,14,00,00,00,01,01,00,00,00,00,00,05,0b,00,00,00,01,01,00,00,00,00,00,05,12,00,00,00,01,01,00,00,00,00,00,05,12,00,00,00".Split(','), s => Convert.ToByte(s, 16)), RegistryValueKind.Binary);

                        RegistryHelp.Write(Registry.LocalMachine, diagsvc, "DependOnService", new[] { "RpcSs" }, RegistryValueKind.MultiString);
                        RegistryHelp.Write(Registry.LocalMachine, diagsvc, "Description", @"@%systemroot%\system32\DiagSvc.dll,-101", RegistryValueKind.String);
                        RegistryHelp.Write(Registry.LocalMachine, diagsvc, "DisplayName", @"@%systemroot%\system32\DiagSvc.dll,-100", RegistryValueKind.String);
                        RegistryHelp.Write(Registry.LocalMachine, diagsvc, "ErrorControl", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, diagsvc, "FailureActions", Array.ConvertAll("80,51,01,00,00,00,00,00,00,00,00,00,03,00,00,00,14,00,00,00,01,00,00,00,30,75,00,00,01,00,00,00,30,75,00,00,00,00,00,00,00,00,00,00".Split(','), s => Convert.ToByte(s, 16)), RegistryValueKind.Binary);
                        RegistryHelp.Write(Registry.LocalMachine, diagsvc, "ImagePath", @"%SystemRoot%\System32\svchost.exe -k diagnostics", RegistryValueKind.ExpandString);
                        RegistryHelp.Write(Registry.LocalMachine, diagsvc, "ObjectName", "LocalSystem", RegistryValueKind.String);
                        RegistryHelp.Write(Registry.LocalMachine, diagsvc, "RequiredPrivileges", new[] { "SeTcbPrivilege", "nSeTakeOwnershipPrivilege", "nSeDebugPrivilege", "nSeBackupPrivilege", "nSeImpersonatePrivilege", "nSeLoadDriverPrivilege", "nSeRestorePrivilege", "nSeManageVolumePrivilege" }, RegistryValueKind.MultiString);
                        RegistryHelp.Write(Registry.LocalMachine, diagsvc, "ServiceSidType", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, diagsvc, "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, diagsvc, "Type", 32, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, $@"{diagsvc}\Parameters", "ServiceDll", @"%systemroot%\system32\DiagSvc.dll", RegistryValueKind.ExpandString);
                        RegistryHelp.Write(Registry.LocalMachine, $@"{diagsvc}\Parameters", "ServiceDllUnloadOnStop", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, $@"{diagsvc}\Parameters", "ServiceMain", "ServiceMain", RegistryValueKind.String);
                        RegistryHelp.Write(Registry.LocalMachine, $@"{diagsvc}\TriggerInfo\0", "Action", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, $@"{diagsvc}\TriggerInfo\0", "Data0", Array.ConvertAll("46,00,44,00,44,00,34,00,35,00,39,00,32,00,34,00,2d,00,37,00,38,00,34,00,41,00,2d,00,34,00,39,00,39,00,43,00,2d,00,41,00,45,00,45,00,39,00,2d,00,30,00,38,00,31,00,33,00,38,00,35,00,30,00,43,00,45,00,31,00,38,00,32,00,00,00".Split(','), s => Convert.ToByte(s, 16)), RegistryValueKind.Binary);
                        RegistryHelp.Write(Registry.LocalMachine, $@"{diagsvc}\TriggerInfo\0", "DataType0", 2, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, $@"{diagsvc}\TriggerInfo\0", "GUID", Array.ConvertAll("67,d1,90,bc,70,94,39,41,a9,ba,be,0b,bb,f5,b7,4d".Split(','), s => Convert.ToByte(s, 16)), RegistryValueKind.Binary);
                        RegistryHelp.Write(Registry.LocalMachine, $@"{diagsvc}\TriggerInfo\0", "Type", 6, RegistryValueKind.DWord);
                    }
                    break;
                case "TglButton16":
                    CommandExecutor.RunCommandAsTrustedInstaller($@"/c reg add HKLM\SYSTEM\CurrentControlSet\Services\diagnosticshub.standardcollector.service /t REG_DWORD /v Start /d {(isDisabled ? "4" : "3")} /f");
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\diagnosticshub.standardcollector.service", "Start", isDisabled ? 4 : 3, RegistryValueKind.DWord, true);
                    break;
                case "TglButton17":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\NvTelemetryContainer", "Start", isDisabled ? 4 : 2, RegistryValueKind.DWord);
                    SetTaskState(!isDisabled, nvidiaTasks);
                    break;
                case "TglButton18":
                    if (isDisabled)
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
