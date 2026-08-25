using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTweak.Modules.Common;
using GTweak.Modules.Helpers;
using GTweak.Modules.Managers;
using GTweak.Properties;
using Microsoft.Win32;

namespace GTweak.Modules.Tweaks
{
    internal enum ConfidentialityToggle
    {
        TargetedAdvertising = 1,

        DataSynchronization,

        WindowsTelemetry,

        SchedulerDataCollection,

        InstalledAppsData,

        AppUsageStatistics,

        HandwritingData,


        [PostAction(NotificationManager.AlertType.Restart)]
        HardwareConfigurationData,

        HiddenMicrosoftDomains,

        UserLocationTracking,

        FeedbackRequests,

        SpeechSynthesisUpdates,

        HiddenSystemMonitoring,

        SystemExperiments,

        [PostAction(NotificationManager.AlertType.Restart)]
        CovertDataCollectionServices,

        WindowsEventLogging,

        NvidiaTelemetry,

        UserBehaviorRecording,

        OfflineMapsUpdates,

        [PostAction(NotificationManager.AlertType.Restart)]
        IntelTelemetry
    }

    internal sealed class ConfidentialityTweaks : FirewallManager
    {
        internal readonly static Dictionary<string, object> ControlStates = new Dictionary<string, object>();
        private readonly ControlWriterManager _сontrolWriter = new ControlWriterManager(ControlStates);
        private readonly Dictionary<ConfidentialityToggle, (Func<bool> Check, Action<bool> Apply)> _tglTweaks;

        public ConfidentialityTweaks()
        {
            _tglTweaks = new Dictionary<ConfidentialityToggle, (Func<bool> Check, Action<bool> Apply)>
            {
                [ConfidentialityToggle.TargetedAdvertising] = (
                    Check: () =>
                    {
                        return RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PolicyManager\current\device\Bluetooth", "AllowAdvertising", "0");
                    },
                    Apply: (state) =>
                    {
                        if (state)
                        {
                            RegistryHelper.DeleteValue(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\PolicyManager\current\device\Bluetooth", "AllowAdvertising");
                        }
                        else
                        {
                            RegistryHelper.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\PolicyManager\current\device\Bluetooth", "AllowAdvertising", 0, RegistryValueKind.DWord);
                        }
                    }
                ),

                [ConfidentialityToggle.DataSynchronization] = (
                    Check: () =>
                    {
                        return RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Accessibility", "Enabled", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SettingSync\Groups\BrowserSettings", "Enabled", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Credentials", "Enabled", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Language", "Enabled", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Personalization", "Enabled", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Windows", "Enabled", "0");
                    },
                    Apply: (state) =>
                    {
                        if (state)
                        {
                            RegistryHelper.DeleteValue(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\BrowserSettings", "Enabled");
                            RegistryHelper.DeleteValue(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Credentials", "Enabled");
                            RegistryHelper.DeleteValue(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Language", "Enabled");
                            RegistryHelper.DeleteValue(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Personalization", "Enabled");
                            RegistryHelper.DeleteValue(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Windows", "Enabled");
                            RegistryHelper.DeleteValue(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Accessibility", "Enabled");
                        }
                        else
                        {
                            RegistryHelper.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\BrowserSettings", "Enabled", 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Credentials", "Enabled", 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Language", "Enabled", 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Personalization", "Enabled", 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Windows", "Enabled", 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Accessibility", "Enabled", 0, RegistryValueKind.DWord);
                        }
                    }
                ),

                [ConfidentialityToggle.WindowsTelemetry] = (
                   Check: () =>
                   {
                       return RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\WMI\Autologger\Diagtrack-Listener", "Start", "0") ||
                       RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Attachments", "SaveZoneInformation", "1") || IsTaskEnabled(telemetryTasks);
                   },
                   Apply: (state) =>
                   {
                       RegistryHelper.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\WMI\Autologger\Diagtrack-Listener", "Start", state ? 1 : 0, RegistryValueKind.DWord);

                       if (state)
                       {
                           RegistryHelper.DeleteValue(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Attachments", "SaveZoneInformation");
                       }
                       else
                       {
                           RegistryHelper.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Attachments", "SaveZoneInformation", 1, RegistryValueKind.DWord);
                       }

                       SetTaskState(state, telemetryTasks);
                   }
                ),

                [ConfidentialityToggle.SchedulerDataCollection] = (
                    Check: () => IsTaskEnabled(dataCollectTasks),
                    Apply: (state) => SetTaskState(state, dataCollectTasks)
                ),

                [ConfidentialityToggle.InstalledAppsData] = (
                    Check: () => RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\AppCompat", "DisableInventory", "1") || IsTaskEnabled(appExpInventoryTasks),
                    Apply: (state) =>
                    {
                        if (state)
                        {
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppCompat", "DisableInventory");
                        }
                        else
                        {
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppCompat", "DisableInventory", 1, RegistryValueKind.DWord);
                        }

                        SetTaskStateOwner(state, appExpInventoryTasks);
                    }
                ),

                [ConfidentialityToggle.AppUsageStatistics] = (
                    Check: () =>
                    {
                        return RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\DataCollection", "AllowTelemetry", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\AppCompat", "AITEnable", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowDeviceNameInTelemetry", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackProgs", "0") || IsTaskEnabled(appExpUsageTasks);
                    },
                    Apply: (state) =>
                    {
                        RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\DataCollection", "AllowTelemetry", state ? 1 : 0, RegistryValueKind.DWord);
                        RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppCompat", "AITEnable", state ? 1 : 0, RegistryValueKind.DWord);
                        RegistryHelper.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackProgs", state ? 1 : 0, RegistryValueKind.DWord);

                        if (state)
                        {
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowDeviceNameInTelemetry");
                        }
                        else
                        {
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowDeviceNameInTelemetry", 0, RegistryValueKind.DWord);
                        }

                        SetTaskStateOwner(state, appExpUsageTasks);
                    }
                ),

                [ConfidentialityToggle.HandwritingData] = (
                    Check: () =>
                    {
                        return RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\TabletPC", "PreventHandwritingDataSharing", "1") ||
                        RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\HandwritingErrorReports", "PreventHandwritingErrorReports", "1") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Input\TIPC", "Enabled", "0");
                    },
                    Apply: (state) =>
                    {
                        RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\HandwritingErrorReports", "PreventHandwritingErrorReports", state ? 0 : 1, RegistryValueKind.DWord);
                        RegistryHelper.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Input\TIPC", "Enabled", state ? 1 : 0, RegistryValueKind.DWord);

                        if (state)
                        {
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\TabletPC", "PreventHandwritingDataSharing");
                        }
                        else
                        {
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\TabletPC", "PreventHandwritingDataSharing", 1, RegistryValueKind.DWord);
                        }
                    }
                ),

                [ConfidentialityToggle.HardwareConfigurationData] = (
                    Check: () => RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\SQMClient\Windows", "CEIPEnable", "0") || IsTaskEnabled(ceipTasks),
                    Apply: (state) =>
                    {
                        if (state)
                        {
                            RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\SQMClient");
                        }
                        else
                        {
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\SQMClient\Windows", "CEIPEnable", 0, RegistryValueKind.DWord);
                        }

                        SetTaskState(state, ceipTasks);
                    }
                ),

                [ConfidentialityToggle.HiddenMicrosoftDomains] = (
                    Check: () => IsDefaultHosts(),
                    Apply: (state) =>
                    {
                        BlockSpyDomain(state);

                        Task.Run(delegate
                        {
                            try
                            {
                                if (state)
                                {
                                    if (File.Exists(PathTargets.Files.Hosts.Backup))
                                    {
                                        File.Copy(PathTargets.Files.Hosts.Backup, PathTargets.Files.Hosts.Original, true);
                                        File.Delete(PathTargets.Files.Hosts.Backup);
                                    }
                                    else
                                    {
                                        File.WriteAllText(PathTargets.Files.Hosts.Original, string.Empty);
                                    }
                                }
                                else
                                {
                                    File.Copy(PathTargets.Files.Hosts.Original, PathTargets.Files.Hosts.Backup, true);

                                    string existingText = File.Exists(PathTargets.Files.Hosts.Original) ? File.ReadAllText(PathTargets.Files.Hosts.Original) : string.Empty;

                                    HashSet<string> existingEntries = new HashSet<string>(existingText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(line => line.Trim()), StringComparer.OrdinalIgnoreCase);

                                    StringBuilder blocklist = new StringBuilder();

                                    foreach (string line in Resources.Blocklist.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                                    {
                                        string trimmedLine = line.Trim();
                                        if (!string.IsNullOrEmpty(trimmedLine) && existingEntries.Add(trimmedLine))
                                        {
                                            blocklist.AppendLine(trimmedLine);
                                        }
                                    }

                                    if (blocklist.Length > 0)
                                    {
                                        FileInfo fileInfo = new FileInfo(PathTargets.Files.Hosts.Original);

                                        if (fileInfo.Exists && fileInfo.IsReadOnly)
                                        {
                                            fileInfo.IsReadOnly = false;
                                        }

                                        string prefix = string.Empty;
                                        if (existingText.Length > 0)
                                        {
                                            string lastLine = existingText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault()?.Trim() ?? string.Empty;
                                            prefix = lastLine.StartsWith("0.0.0.0") ? existingText.EndsWith("\n") ? string.Empty : Environment.NewLine : existingText.EndsWith("\n") ? Environment.NewLine : Environment.NewLine + Environment.NewLine;
                                        }

                                        File.AppendAllText(PathTargets.Files.Hosts.Original, $"{prefix}{blocklist.ToString().TrimEnd()}");
                                    }
                                }
                            }
                            catch (Exception ex) { ErrorLogger.LogDebug(ex); }
                        });
                    }
                ),

                [ConfidentialityToggle.UserLocationTracking] = (
                    Check: () =>
                    {
                        return RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors", "DisableLocation", "1") ||
                        RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors", "DisableLocationScripting", "1") ||
                        RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors", "DisableWindowsLocationProvider", "1");
                    },
                    Apply: (state) =>
                    {
                        if (state)
                        {
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors", "DisableLocation");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors", "DisableLocationScripting");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors", "DisableWindowsLocationProvider");
                        }
                        else
                        {
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors", "DisableLocation", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors", "DisableLocationScripting", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors", "DisableWindowsLocationProvider", 1, RegistryValueKind.DWord);
                        }
                    }
                ),

                [ConfidentialityToggle.FeedbackRequests] = (
                    Check: () =>
                    {
                        return RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Siuf\Rules", "NumberOfSIUFInPeriod", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Siuf\Rules", "PeriodInNanoSeconds", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DataCollection", "DoNotShowFeedbackNotifications", "1") || IsTaskEnabled(feedbackTasks);
                    },
                    Apply: (state) =>
                    {
                        if (state)
                        {
                            RegistryHelper.DeleteValue(Registry.CurrentUser, @"SOFTWARE\Microsoft\Siuf\Rules", "NumberOfSIUFInPeriod");
                            RegistryHelper.DeleteValue(Registry.CurrentUser, @"SOFTWARE\Microsoft\Siuf\Rules", "PeriodInNanoSeconds");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "DoNotShowFeedbackNotifications");
                        }
                        else
                        {
                            RegistryHelper.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Siuf\Rules", "NumberOfSIUFInPeriod", 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Siuf\Rules", "PeriodInNanoSeconds", 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "DoNotShowFeedbackNotifications", 1, RegistryValueKind.DWord);
                        }

                        SetTaskState(state, feedbackTasks);
                    }
                ),

                [ConfidentialityToggle.SpeechSynthesisUpdates] = (
                    Check: () => RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Speech", "AllowSpeechModelUpdate", "0") || IsTaskEnabled(speechTasks),
                    Apply: (state) =>
                    {
                        if (state)
                        {
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Speech", "AllowSpeechModelUpdate");
                        }
                        else
                        {
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Speech", "AllowSpeechModelUpdate", 0, RegistryValueKind.DWord);
                        }

                        SetTaskState(state, speechTasks);
                    }
                ),

                [ConfidentialityToggle.HiddenSystemMonitoring] = (
                    Check: () => RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\CDPUserSvc", "Start", "4"),
                    Apply: (state) => RegistryHelper.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\CDPUserSvc", "Start", state ? 2 : 4, RegistryValueKind.DWord)
                ),

                [ConfidentialityToggle.SystemExperiments] = (
                    Check: () => RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PolicyManager\current\device\System", "AllowExperimentation", "0"),
                    Apply: (state) =>
                    {
                        if (state)
                        {
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\PolicyManager\current\device\System", "AllowExperimentation");
                        }
                        else
                        {
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\PolicyManager\current\device\System", "AllowExperimentation", 0, RegistryValueKind.DWord);
                        }
                    }
                ),

                [ConfidentialityToggle.CovertDataCollectionServices] = (
                    Check: () =>
                    {
                        return RegistryHelper.KeyExists(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\DiagTrack") ||
                        RegistryHelper.KeyExists(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\dmwappushservice") ||
                        RegistryHelper.KeyExists(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\diagsvc");
                    },
                    Apply: (state) =>
                    {
                        (string diagTrack, string dmwappushservice, string diagsvc) =
                        (
                            @"SYSTEM\CurrentControlSet\Services\DiagTrack",
                            @"SYSTEM\CurrentControlSet\Services\dmwappushservice",
                            @"SYSTEM\CurrentControlSet\Services\diagsvc"
                        );

                        if (state)
                        {
                            RegistryHelper.Write(Registry.LocalMachine, diagTrack, "DependOnService", new[] { "RpcSs" }, RegistryValueKind.MultiString);
                            RegistryHelper.Write(Registry.LocalMachine, diagTrack, "Description", @"@%SystemRoot%\system32\diagtrack.dll,-3002", RegistryValueKind.String);
                            RegistryHelper.Write(Registry.LocalMachine, diagTrack, "DisplayName", @"@%SystemRoot%\system32\diagtrack.dll,-3001", RegistryValueKind.String);
                            RegistryHelper.Write(Registry.LocalMachine, diagTrack, "ErrorControl", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, diagTrack, "FailureActions", Array.ConvertAll("80,51,01,00,00,00,00,00,00,00,00,00,03,00,00,00,14,00,00,00,01,00,00,00,30,75,00,00,01,00,00,00,30,75,00,00,00,00,00,00,00,00,00,00".Split(','), s => Convert.ToByte(s, 16)), RegistryValueKind.Binary);
                            RegistryHelper.Write(Registry.LocalMachine, diagTrack, "ImagePath", @"%SystemRoot%\System32\svchost.exe -k utcsvc -p", RegistryValueKind.ExpandString);
                            RegistryHelper.Write(Registry.LocalMachine, diagTrack, "ObjectName", "LocalSystem", RegistryValueKind.String);
                            RegistryHelper.Write(Registry.LocalMachine, diagTrack, "RequiredPrivileges", new[] { "SeChangeNotifyPrivilege", "SeCreateGlobalPrivilege", "SeAssignPrimaryTokenPrivilege", "SeImpersonatePrivilege", "SeSystemProfilePrivilege", "SeTcbPrivilege", "SeDebugPrivilege", "SeSecurityPrivilege" }, RegistryValueKind.MultiString);
                            RegistryHelper.Write(Registry.LocalMachine, diagTrack, "ServiceSidType", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, diagTrack, "Start", 2, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, diagTrack, "Type", 16, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, $@"{diagTrack}\Parameters", "ServiceDll", @"%SystemRoot%\system32\diagtrack.dll", RegistryValueKind.ExpandString);
                            RegistryHelper.Write(Registry.LocalMachine, $@"{diagTrack}\Parameters", "ServiceDllUnloadOnStop", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, $@"{diagTrack}\Parameters", "ServiceMain", "ServiceMain", RegistryValueKind.String);

                            RegistryHelper.Write(Registry.LocalMachine, dmwappushservice, "DelayedAutoStart", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, dmwappushservice, "DependOnService", new[] { "rpcss" }, RegistryValueKind.MultiString);
                            RegistryHelper.Write(Registry.LocalMachine, dmwappushservice, "Description", @"@%SystemRoot%\system32\dmwappushsvc.dll,-201", RegistryValueKind.String);
                            RegistryHelper.Write(Registry.LocalMachine, dmwappushservice, "DisplayName", @"@%SystemRoot%\system32\dmwappushsvc.dll,-200", RegistryValueKind.String);
                            RegistryHelper.Write(Registry.LocalMachine, dmwappushservice, "ErrorControl", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, dmwappushservice, "FailureActions", Array.ConvertAll("80,51,01,00,00,00,00,00,00,00,00,00,04,00,00,00,14,00,00,00,01,00,00,00,10,27,00,00,01,00,00,00,10,27,00,00,01,00,00,00,10,27,00,00,00,00,00,00,10,27,00,00".Split(','), s => Convert.ToByte(s, 16)), RegistryValueKind.Binary);
                            RegistryHelper.Write(Registry.LocalMachine, dmwappushservice, "ImagePath", @"%SystemRoot%\system32\svchost.exe -k netsvcs -p", RegistryValueKind.ExpandString);
                            RegistryHelper.Write(Registry.LocalMachine, dmwappushservice, "ObjectName", "LocalSystem", RegistryValueKind.String);
                            RegistryHelper.Write(Registry.LocalMachine, dmwappushservice, "ServiceSidType", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, dmwappushservice, "Start", 3, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, dmwappushservice, "Type", 20, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, dmwappushservice, "StateFlags", 3, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, $@"{dmwappushservice}\Parameters", "IdleTimeout(sec)", 120, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, $@"{dmwappushservice}\Parameters", "ServiceDll", @"%SystemRoot%\system32\dmwappushsvc.dll", RegistryValueKind.ExpandString);
                            RegistryHelper.Write(Registry.LocalMachine, $@"{dmwappushservice}\Parameters", "ServiceDllUnloadOnStop", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, $@"{dmwappushservice}\Parameters", "ServiceMain", "ServiceMain", RegistryValueKind.String);
                            RegistryHelper.Write(Registry.LocalMachine, $@"{dmwappushservice}\TriggerInfo\0", "Action", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, $@"{dmwappushservice}\TriggerInfo\0", "Data0", Array.ConvertAll("37,00,39,00,35,00,42,00,36,00,42,00,46,00,39,00,2d,00,39,00,37,00,42,00,36,00,2d,00,34,00,46,00,38,00,39,00,2d,00,42,00,44,00,38,00,44,00,2d,00,32,00,46,00,34,00,32,00,42,00,42,00,42,00,45,00,39,00,39,00,36,00,45,00,00,00".Split(','), s => Convert.ToByte(s, 16)), RegistryValueKind.Binary);
                            RegistryHelper.Write(Registry.LocalMachine, $@"{dmwappushservice}\TriggerInfo\0", "DataType0", 2, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, $@"{dmwappushservice}\TriggerInfo\0", "GUID", Array.ConvertAll("67,d1,90,bc,70,94,39,41,a9,ba,be,0b,bb,f5,b7,4d".Split(','), s => Convert.ToByte(s, 16)), RegistryValueKind.Binary);
                            RegistryHelper.Write(Registry.LocalMachine, $@"{dmwappushservice}\TriggerInfo\0", "Type", 6, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, $@"{dmwappushservice}\TriggerInfo\1", "Action", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, $@"{dmwappushservice}\TriggerInfo\1", "Data0", Array.ConvertAll("75,90,bc,a3,28,00,92,13".Split(','), s => Convert.ToByte(s, 16)), RegistryValueKind.Binary);
                            RegistryHelper.Write(Registry.LocalMachine, $@"{dmwappushservice}\TriggerInfo\1", "DataType0", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, $@"{dmwappushservice}\TriggerInfo\1", "GUID", Array.ConvertAll("16,28,7a,2d,5e,0c,fc,45,9c,e7,57,0e,5e,cd,e9,c9".Split(','), s => Convert.ToByte(s, 16)), RegistryValueKind.Binary);
                            RegistryHelper.Write(Registry.LocalMachine, $@"{dmwappushservice}\TriggerInfo\1", "Type", 7, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, $@"{dmwappushservice}\Security", "Security", Array.ConvertAll("01,00,04,80,b0,00,00,00,bc,00,00,00,00,00,00,00,14,00,00,00,02,00,9c,00,07,00,00,00,00,00,14,00,8d,01,02,00,01,01,00,00,00,00,00,05,04,00,00,00,00,00,14,00,8d,01,02,00,01,01,00,00,00,00,00,05,06,00,00,00,00,00,14,00,ff,01,0f,00,01,01,00,00,00,00,00,05,12,00,00,00,00,00,18,00,ff,01,0f,00,01,02,00,00,00,00,00,05,20,00,00,00,20,02,00,00,00,00,18,00,14,00,00,00,01,02,00,00,00,00,00,0f,02,00,00,00,01,00,00,00,00,00,14,00,14,00,00,00,01,01,00,00,00,00,00,05,04,00,00,00,00,00,14,00,14,00,00,00,01,01,00,00,00,00,00,05,0b,00,00,00,01,01,00,00,00,00,00,05,12,00,00,00,01,01,00,00,00,00,00,05,12,00,00,00".Split(','), s => Convert.ToByte(s, 16)), RegistryValueKind.Binary);

                            RegistryHelper.Write(Registry.LocalMachine, diagsvc, "DependOnService", new[] { "RpcSs" }, RegistryValueKind.MultiString);
                            RegistryHelper.Write(Registry.LocalMachine, diagsvc, "Description", @"@%systemroot%\system32\DiagSvc.dll,-101", RegistryValueKind.String);
                            RegistryHelper.Write(Registry.LocalMachine, diagsvc, "DisplayName", @"@%systemroot%\system32\DiagSvc.dll,-100", RegistryValueKind.String);
                            RegistryHelper.Write(Registry.LocalMachine, diagsvc, "ErrorControl", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, diagsvc, "FailureActions", Array.ConvertAll("80,51,01,00,00,00,00,00,00,00,00,00,03,00,00,00,14,00,00,00,01,00,00,00,30,75,00,00,01,00,00,00,30,75,00,00,00,00,00,00,00,00,00,00".Split(','), s => Convert.ToByte(s, 16)), RegistryValueKind.Binary);
                            RegistryHelper.Write(Registry.LocalMachine, diagsvc, "ImagePath", @"%SystemRoot%\System32\svchost.exe -k diagnostics", RegistryValueKind.ExpandString);
                            RegistryHelper.Write(Registry.LocalMachine, diagsvc, "ObjectName", "LocalSystem", RegistryValueKind.String);
                            RegistryHelper.Write(Registry.LocalMachine, diagsvc, "RequiredPrivileges", new[] { "SeTcbPrivilege", "SeTakeOwnershipPrivilege", "SeDebugPrivilege", "SeBackupPrivilege", "SeImpersonatePrivilege", "SeLoadDriverPrivilege", "SeRestorePrivilege", "SeManageVolumePrivilege" }, RegistryValueKind.MultiString);
                            RegistryHelper.Write(Registry.LocalMachine, diagsvc, "ServiceSidType", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, diagsvc, "Start", 3, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, diagsvc, "Type", 32, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, $@"{diagsvc}\Parameters", "ServiceDll", @"%systemroot%\system32\DiagSvc.dll", RegistryValueKind.ExpandString);
                            RegistryHelper.Write(Registry.LocalMachine, $@"{diagsvc}\Parameters", "ServiceDllUnloadOnStop", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, $@"{diagsvc}\Parameters", "ServiceMain", "ServiceMain", RegistryValueKind.String);
                            RegistryHelper.Write(Registry.LocalMachine, $@"{diagsvc}\TriggerInfo\0", "Action", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, $@"{diagsvc}\TriggerInfo\0", "Data0", Array.ConvertAll("46,00,44,00,44,00,34,00,35,00,39,00,32,00,34,00,2d,00,37,00,38,00,34,00,41,00,2d,00,34,00,39,00,39,00,43,00,2d,00,41,00,45,00,45,00,39,00,2d,00,30,00,38,00,31,00,33,00,38,00,35,00,30,00,43,00,45,00,31,00,38,00,32,00,00,00".Split(','), s => Convert.ToByte(s, 16)), RegistryValueKind.Binary);
                            RegistryHelper.Write(Registry.LocalMachine, $@"{diagsvc}\TriggerInfo\0", "DataType0", 2, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, $@"{diagsvc}\TriggerInfo\0", "GUID", Array.ConvertAll("67,d1,90,bc,70,94,39,41,a9,ba,be,0b,bb,f5,b7,4d".Split(','), s => Convert.ToByte(s, 16)), RegistryValueKind.Binary);
                            RegistryHelper.Write(Registry.LocalMachine, $@"{diagsvc}\TriggerInfo\0", "Type", 6, RegistryValueKind.DWord);
                        }
                        else
                        {
                            foreach (string path in new[] { diagTrack, dmwappushservice, diagsvc })
                            {
                                RegistryHelper.DeleteFolderTree(Registry.LocalMachine, path);
                            }
                        }
                    }
                ),

                [ConfidentialityToggle.WindowsEventLogging] = (
                    Check: () => RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\diagnosticshub.standardcollector.service", "Start", "4"),
                    Apply: (state) =>
                    {
                        CommandExecutor.RunCommandAsTrustedInstaller($@"/c reg add HKLM\SYSTEM\CurrentControlSet\Services\diagnosticshub.standardcollector.service /t REG_DWORD /v Start /d {(state ? "3" : "4")} /f");
                        RegistryHelper.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\diagnosticshub.standardcollector.service", "Start", state ? 3 : 4, RegistryValueKind.DWord, true);
                    }
                ),


                [ConfidentialityToggle.NvidiaTelemetry] = (
                    Check: () =>
                    {
                        return RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\NvTelemetryContainer", "Start", "4") || IsTaskEnabled(nvidiaTasks);
                    },
                    Apply: (state) =>
                    {
                        RegistryHelper.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\NvTelemetryContainer", "Start", state ? 2 : 4, RegistryValueKind.DWord);
                        SetTaskState(state, nvidiaTasks);
                    }
                ),

                [ConfidentialityToggle.UserBehaviorRecording] = (
                    Check: () =>
                    {
                        return RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\AppCompat", "DisableUAR", "1") ||
                        RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Personalization", "NoLockScreenCamera", "1");
                    },
                    Apply: (state) =>
                    {
                        if (state)
                        {
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppCompat", "DisableUAR");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Personalization", "NoLockScreenCamera");
                        }
                        else
                        {
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppCompat", "DisableUAR", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Personalization", "NoLockScreenCamera", 1, RegistryValueKind.DWord);
                        }
                    }
                ),

                [ConfidentialityToggle.OfflineMapsUpdates] = (
                    Check: () =>
                    {
                        return RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\Maps", "MapUpdate", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\Maps", "AutoUpdateEnabled", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Maps", "AutoDownloadAndUpdateMapData", "0") || IsTaskEnabled(mapsTasks);
                    },
                    Apply: (state) =>
                    {
                        if (state)
                        {
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SYSTEM\Maps", "AutoUpdateEnabled");
                            RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Maps");
                        }
                        else
                        {
                            RegistryHelper.Write(Registry.LocalMachine, @"SYSTEM\Maps", "AutoUpdateEnabled", 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Maps", "AutoDownloadAndUpdateMapData", 0, RegistryValueKind.DWord);
                        }

                        RegistryHelper.Write(Registry.LocalMachine, @"SYSTEM\Maps", "MapUpdate", state ? 1 : 0, RegistryValueKind.DWord);

                        SetTaskState(state, mapsTasks);
                    }
                ),

                [ConfidentialityToggle.IntelTelemetry] = (
                    Check: () =>
                    {
                        return (RegistryHelper.ValueExists(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Telemetry", "Start") &&
                        RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Telemetry", "Start", "4")) || IsTaskEnabled(intelTask);
                    },
                    Apply: (state) =>
                    {
                        if (RegistryHelper.ValueExists(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Telemetry", "Start"))
                        {
                            RegistryHelper.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\Telemetry", "Start", state ? 2 : 4, RegistryValueKind.DWord);
                        }

                        SetTaskState(state, intelTask);
                    }
                ),
            };
        }

        internal void CheckAll()
        {
            foreach (var tweak in _tglTweaks)
            {
                _сontrolWriter.ToggleButton[(int)tweak.Key] = tweak.Value.Check();
            }
        }

        internal void Apply(string tweakName, bool state)
        {
            INIManager.TempWrite(INIManager.TempTweaksConf, tweakName, state);

            if (tweakName.StartsWith("TglButton") && int.TryParse(tweakName.Substring(9), out int index))
            {
                ConfidentialityToggle tweakKey = (ConfidentialityToggle)index;

                if (_tglTweaks.TryGetValue(tweakKey, out var action))
                {
                    Task.Run(() => action.Apply(state));
                }
            }
        }

        private bool IsDefaultHosts()
        {
            try
            {
                HashSet<string> resourceEntries = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (string line in Resources.Blocklist.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string trimmed = line.Trim();
                    if (trimmed.StartsWith("0.0.0.0"))
                    {
                        resourceEntries.Add(trimmed);
                    }
                }

                if (resourceEntries.Count == 0)
                {
                    return true;
                }

                HashSet<string> fileEntries = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (string line in File.ReadLines(PathTargets.Files.Hosts.Original))
                {
                    string trimmed = line.Trim();
                    if (trimmed.StartsWith("0.0.0.0"))
                    {
                        fileEntries.Add(trimmed);
                    }
                }

                return !resourceEntries.IsSubsetOf(fileEntries);
            }
            catch { return true; }
        }
    }
}
