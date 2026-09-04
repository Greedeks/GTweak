using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GTweak.Modules.Common;
using GTweak.Modules.Configuration;
using GTweak.Modules.Helpers;
using GTweak.Modules.Managers;
using GTweak.Modules.Tweaks.Defender;
using GTweak.Windows;
using Microsoft.Win32;

namespace GTweak.Modules.Tweaks
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct UIAction
    {
        internal const uint SPI_SETMOUSESPEED = 0x0071;
        internal const uint SPI_SETKEYBOARDDELAY = 0x0017;
        internal const uint SPI_SETKEYBOARDSPEED = 0x000B;
        internal const uint SPI_SETMOUSE = 0x0004;
    };

    internal sealed class SystemTweaks : FirewallManager
    {
        internal enum Slider
        {
            MouseSensitivity,
            KeyboardDelay,
            KeyboardSpeed
        }

        internal enum Toggle
        {
            PointerPrecision,

            [PostAction(NotificationManager.AlertType.Logout)]

            StickyKeysFilter,

            WindowsDefender,

            [PostAction(NotificationManager.AlertType.Restart)]
            UserAccountControl,

            [PostAction(NotificationManager.AlertType.Restart)]
            SecurityNotifications,

            StoreAutoUpdate,

            [PostAction(NotificationManager.AlertType.Restart)]
            RealtekAudioDelay,

            LockScreenTimeout,

            HibernationFastStartup,

            AutoEndTasks,

            ExeLaunchWarnings,

            [PostAction(NotificationManager.AlertType.Restart)]
            MemoryDiagnostics,

            [PostAction(NotificationManager.AlertType.Restart)]
            NetworkProtocols,

            [PostAction(NotificationManager.AlertType.Restart)]
            FileSystemCache,

            [PostAction(NotificationManager.AlertType.Restart)]
            StartupDelay,

            QuickAccessHistory,

            RemovableMediaAutoplay,

            PowerScheme,

            BluetoothFunction,

            [PostAction(NotificationManager.AlertType.Restart)]
            WindowsFirewall,

            GameMode,

            GameBar,

            [PostAction(NotificationManager.AlertType.Restart)]
            BackgroundApps,

            ReservedStorage,

            [PostAction(NotificationManager.AlertType.Restart)]
            DynamicTickHpet,

            HealthCheck,

            [PostAction(NotificationManager.AlertType.Restart)]
            InsiderTasks,

            SystemDriveDefrag,

            PauseWindowsUpdates,

            [PostAction(NotificationManager.AlertType.Restart)]
            MultiPlaneOverlay
        }

        private static bool _isNetshState = false, _isTickState = false;
        private static string _currentPowerGuid = string.Empty;

        internal readonly static Dictionary<string, object> ControlStates = new Dictionary<string, object>();
        private readonly ControlWriterManager _controlWriter = new ControlWriterManager(ControlStates);
        private readonly Dictionary<Slider, (Func<double> Check, Action<uint> Apply)> _sliderMappings;
        private readonly Dictionary<Toggle, (Func<bool> Check, Action<bool, bool> Apply)> _toggleMappings;

        public SystemTweaks()
        {
            _currentPowerGuid = RegistryHelper.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes", "ActivePowerScheme", string.Empty);

            _sliderMappings = new Dictionary<Slider, (Func<double> Check, Action<uint> Apply)>
            {
                [Slider.MouseSensitivity] = (
                    Check: () => RegistryHelper.GetValue<double>(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseSensitivity", 10),
                    Apply: (value) =>
                    {
                        SystemParametersInfo(UIAction.SPI_SETMOUSESPEED, value, value, 2);
                        RegistryHelper.Write(Registry.CurrentUser, @"Control Panel\Mouse", "MouseSensitivity", value, RegistryValueKind.String);
                    }
                ),

                [Slider.KeyboardDelay] = (
                    Check: () => RegistryHelper.GetValue<double>(@"HKEY_CURRENT_USER\Control Panel\Keyboard", "KeyboardDelay", 1),
                    Apply: (value) =>
                    {
                        SystemParametersInfo(UIAction.SPI_SETKEYBOARDDELAY, value, value, 2);
                        RegistryHelper.Write(Registry.CurrentUser, @"Control Panel\Keyboard", "KeyboardDelay", value, RegistryValueKind.String);
                    }
                ),

                [Slider.KeyboardSpeed] = (
                    Check: () => RegistryHelper.GetValue<double>(@"HKEY_CURRENT_USER\Control Panel\Keyboard", "KeyboardSpeed", 31),
                    Apply: (value) =>
                    {
                        SystemParametersInfo(UIAction.SPI_SETKEYBOARDSPEED, value, value, 2);
                        RegistryHelper.Write(Registry.CurrentUser, @"Control Panel\Keyboard", "KeyboardSpeed", value, RegistryValueKind.String);
                    }
                ),
            };

            _toggleMappings = new Dictionary<Toggle, (Func<bool> Check, Action<bool, bool> Apply)>
            {
                [Toggle.PointerPrecision] = (
                    Check: () =>
                    {
                        return RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseSpeed", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseThreshold1", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseThreshold2", "0");
                    },
                    Apply: (state, _) =>
                    {
                        SystemParametersInfo(UIAction.SPI_SETMOUSE, 0, state ? new uint[] { 1, 6, 10 } : new uint[3], 2);
                        RegistryHelper.Write(Registry.CurrentUser, @"Control Panel\Mouse", "MouseSpeed", state ? "1" : "0", RegistryValueKind.String);
                        RegistryHelper.Write(Registry.CurrentUser, @"Control Panel\Mouse", "MouseThreshold1", state ? "6" : "0", RegistryValueKind.String);
                        RegistryHelper.Write(Registry.CurrentUser, @"Control Panel\Mouse", "MouseThreshold2", state ? "10" : "0", RegistryValueKind.String);
                    }
                ),

                [Toggle.StickyKeysFilter] = (
                    Check: () =>
                    {
                        return RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Accessibility\StickyKeys", "Flags", "26") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Accessibility\Keyboard Response", "Flags", "26");
                    },
                    Apply: (state, _) =>
                    {
                        RegistryHelper.Write(Registry.CurrentUser, @"Control Panel\Accessibility\StickyKeys", "Flags", state ? "507" : "26", RegistryValueKind.String);
                        RegistryHelper.Write(Registry.CurrentUser, @"Control Panel\Accessibility\Keyboard Response", "Flags", state ? "58" : "26", RegistryValueKind.String);
                    }
                ),

                [Toggle.WindowsDefender] = (
                    Check: () => File.Exists(PathTargets.Targets.Defender.SmartScreen.Normal),
                    Apply: async (state, canShowWindow) =>
                    {
                        BlockWDefender(state);
                        ArchiveManager.Unarchive(PathTargets.Executable.NSudo, Properties.Resources.NSudoLC);
                        if (state == false)
                        {
                            ArchiveManager.Unarchive(PathTargets.Executable.DisablingWD, Properties.Resources.DisablingWD);
                        }

                        try
                        {
                            if (canShowWindow)
                            {
                                OverlayWindow overlayWindow = new OverlayWindow();
                                overlayWindow.Show();

                                BackgroundQueueManager backgroundQueue = new BackgroundQueueManager();
                                await backgroundQueue.QueueTask(() => (state ? NotificationManager.Info("info_wd_noty") : NotificationManager.Warn("warn_wd_noty")).Perform());
                                await backgroundQueue.QueueTask(delegate { DefenderTweaks.SetProtectionState(state); });

                                if (state)
                                {
                                    await backgroundQueue.QueueTask(delegate { NotificationManager.Default().WithDelay(300).Restart(); });
                                }

                                overlayWindow.Close();
                            }
                            else
                            {
                                DefenderTweaks.SetProtectionState(state);
                            }
                        }
                        catch (Exception ex) { ErrorLogger.LogDebug(ex); }
                    }
                ),

                [Toggle.UserAccountControl] = (
                    Check: () =>
                    {
                        return RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "PromptOnSecureDesktop", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableLUA", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableInstallerDetection", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableSecureUIAPaths", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "FilterAdministratorToken", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableVirtualization", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "ConsentPromptBehaviorAdmin", "0");
                    },
                    Apply: (state, _) =>
                    {
                        RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "ConsentPromptBehaviorAdmin", state ? 5 : 0, RegistryValueKind.DWord);
                        RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableInstallerDetection", state ? 1 : 0, RegistryValueKind.DWord);
                        RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableLUA", state ? 1 : 0, RegistryValueKind.DWord);
                        RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableSecureUIAPaths", state ? 1 : 0, RegistryValueKind.DWord);
                        RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableVirtualization", state ? 1 : 0, RegistryValueKind.DWord);
                        RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "FilterAdministratorToken", state ? 1 : 0, RegistryValueKind.DWord);
                        RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "PromptOnSecureDesktop", state ? 1 : 0, RegistryValueKind.DWord);
                    }
                ),

                [Toggle.SecurityNotifications] = (
                    Check: () =>
                    {
                        return RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Notifications\Settings\Windows.SystemToast.SecurityAndMaintenance", "Enabled", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows Defender Security Center\Notifications", "DisableNotifications", "1") ||
                        RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows Defender Security Center\Notifications", "DisableNotifications", "1");
                    },
                    Apply: (state, _) =>
                    {
                        RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Notifications\Settings\Windows.SystemToast.SecurityAndMaintenance", "Enabled", state ? 1 : 0, RegistryValueKind.DWord);
                        if (state)
                        {
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows Defender Security Center\Notifications", "DisableNotifications");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender Security Center\Notifications", "DisableNotifications");
                        }
                        else
                        {
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows Defender Security Center\Notifications", "DisableNotifications", "1", RegistryValueKind.String);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender Security Center\Notifications", "DisableNotifications", 1, RegistryValueKind.DWord);
                        }
                    }
                ),

                [Toggle.StoreAutoUpdate] = (
                    Check: () => RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\WindowsStore", "AutoDownload", "2"),
                    Apply: (state, _) =>
                    {
                        if (state)
                        {
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\WindowsStore", "AutoDownload");
                        }
                        else
                        {
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\WindowsStore", "AutoDownload", 2, RegistryValueKind.DWord);
                        }
                    }
                ),

                [Toggle.RealtekAudioDelay] = (
                    Check: () =>
                    {
                        try
                        {
                            using RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e96c-e325-11ce-bfc1-08002be10318}");
                            if (regKey != null)
                            {
                                foreach (var subKeyName in regKey.GetSubKeyNames())
                                {
                                    if (subKeyName == "Properties")
                                    {
                                        continue;
                                    }

                                    using RegistryKey subKey = regKey.OpenSubKey(subKeyName);
                                    if (subKey != null)
                                    {
                                        if (subKey.GetValue("DriverDesc") is string driverDesc && driverDesc.Equals("Realtek High Definition Audio", StringComparison.OrdinalIgnoreCase))
                                        {
                                            using RegistryKey powerSettingsKey = subKey.OpenSubKey("PowerSettings");
                                            if (powerSettingsKey != null)
                                            {
                                                if (!(powerSettingsKey.GetValue("ConservationIdleTime") is byte[] conservationIdleTime) || !(powerSettingsKey.GetValue("IdlePowerState") is byte[] idlePowerState) || !(powerSettingsKey.GetValue("PerformanceIdleTime") is byte[] performanceIdleTime))
                                                {
                                                    return false;
                                                }

                                                return conservationIdleTime?[0].ToString() != "255" || idlePowerState?[0].ToString() != "0" || performanceIdleTime?[0].ToString() != "255";
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex) { ErrorLogger.LogDebug(ex); }

                        return false;
                    },
                    Apply: (state, _) =>
                    {
                        try
                        {
                            using RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e96c-e325-11ce-bfc1-08002be10318}");
                            if (regKey != null)
                            {
                                foreach (string subKeyName in regKey.GetSubKeyNames())
                                {
                                    using RegistryKey subKey = regKey.OpenSubKey(subKeyName);
                                    if (subKey != null)
                                    {
                                        if (subKey.GetValue("DriverDesc") is string driverDesc && driverDesc.Equals("Realtek High Definition Audio", StringComparison.OrdinalIgnoreCase))
                                        {
                                            RegistryHelper.Write(Registry.LocalMachine, $@"{@"SYSTEM\CurrentControlSet\Control\Class\{4d36e96c-e325-11ce-bfc1-08002be10318}"}\{subKeyName}\PowerSettings", "ConservationIdleTime", state ? new byte[] { 0x0a, 0x00, 0x00, 0x00 } : new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, RegistryValueKind.Binary);
                                            RegistryHelper.Write(Registry.LocalMachine, $@"{@"SYSTEM\CurrentControlSet\Control\Class\{4d36e96c-e325-11ce-bfc1-08002be10318}"}\{subKeyName}\PowerSettings", "IdlePowerState", state ? new byte[] { 0x03, 0x00, 0x00, 0x00 } : Encoding.Unicode.GetBytes("\0\0"), RegistryValueKind.Binary);
                                            RegistryHelper.Write(Registry.LocalMachine, $@"{@"SYSTEM\CurrentControlSet\Control\Class\{4d36e96c-e325-11ce-bfc1-08002be10318}"}\{subKeyName}\PowerSettings", "PerformanceIdleTime", state ? new byte[] { 0x0a, 0x00, 0x00, 0x00 } : new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, RegistryValueKind.Binary);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex) { ErrorLogger.LogDebug(ex); }
                    }
                ),

                [Toggle.LockScreenTimeout] = (
                    Check: () => RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\7516b95f-f776-4464-8c53-06167f40cc99\8EC4B3A5-6868-48c2-BE75-4F3044BE88A7", "Attributes", "2"),
                    Apply: (state, _) => RegistryHelper.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Power\PowerSettings\7516b95f-f776-4464-8c53-06167f40cc99\8EC4B3A5-6868-48c2-BE75-4F3044BE88A7", "Attributes", state ? 1 : 2, RegistryValueKind.DWord)
                ),

                [Toggle.HibernationFastStartup] = (
                    Check: () =>
                    {
                        return RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Power", "HiberbootEnabled", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power", "HibernateEnabled", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FlyoutMenuSettings", "ShowHibernateOption", "0");
                    },
                    Apply: (state, _) =>
                    {
                        RegistryHelper.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Power", "HiberbootEnabled", state ? 1 : 0, RegistryValueKind.DWord);
                        RegistryHelper.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Power", "HibernateEnabled", state ? 1 : 0, RegistryValueKind.DWord);
                        RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FlyoutMenuSettings", "ShowHibernateOption", state ? 1 : 0, RegistryValueKind.DWord);
                        CommandExecutor.RunCommand(@$"/c powercfg.exe -h {(state ? "on" : "off")}");
                    }
                ),

                [Toggle.AutoEndTasks] = (
                    Check: () => RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "AutoEndTasks", "1"),
                    Apply: (state, _) =>
                    {
                        if (state)
                        {
                            RegistryHelper.DeleteValue(Registry.CurrentUser, @"Control Panel\Desktop", "AutoEndTasks");
                        }
                        else
                        {
                            RegistryHelper.Write(Registry.CurrentUser, @"Control Panel\Desktop", "AutoEndTasks", "1", RegistryValueKind.String);
                        }
                    }
                ),

                [Toggle.ExeLaunchWarnings] = (
                    Check: () =>
                    {
                        return RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Internet Explorer\Security", "DisableSecuritySettingsCheck", "1") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings\Zones\3", "1806", "0");
                    },
                    Apply: (state, _) =>
                    {
                        if (state)
                        {
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\Internet Explorer\Security", "DisableSecuritySettingsCheck");
                            RegistryHelper.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Internet Settings\Zones\3", "1806");
                        }
                        else
                        {
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Internet Explorer\Security", "DisableSecuritySettingsCheck", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Internet Settings\Zones\3", "1806", 0, RegistryValueKind.DWord);
                        }
                    }
                ),

                [Toggle.MemoryDiagnostics] = (
                    Check: () => IsTaskEnabled(memoryDiagTasks),
                    Apply: (state, _) => SetTaskState(state, memoryDiagTasks)
                ),

                [Toggle.NetworkProtocols] = (
                    Check: () => _isNetshState,
                    Apply: (state, _) =>
                    {
                        _isNetshState = state;
                        string tunnelState = state ? "default" : "disabled";
                        string privacyState = state ? "enabled" : "disabled";

                        CommandExecutor.RunCommand($"/c netsh int teredo set state {tunnelState} & netsh int ipv6 6to4 set state state = {tunnelState} undoonstop = {tunnelState} & " +
                        $"netsh int ipv6 isatap set state state = {tunnelState} & netsh int ipv6 set privacy state = {privacyState} & " +
                        $"netsh int ipv6 set global randomizeidentifier = {privacyState} & netsh int isatap set state {tunnelState}");
                    }
                ),

                [Toggle.FileSystemCache] = (
                    Check: () => RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "LargeSystemCache", "1"),
                    Apply: (state, _) => RegistryHelper.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "LargeSystemCache", state ? 0 : 1, RegistryValueKind.DWord)
                ),

                [Toggle.StartupDelay] = (
                    Check: () => RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Serialize", "Startupdelayinmsec", "0"),
                    Apply: (state, _) =>
                    {
                        if (state)
                        {
                            RegistryHelper.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Serialize", "Startupdelayinmsec");
                        }
                        else
                        {
                            RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Serialize", "Startupdelayinmsec", 0, RegistryValueKind.DWord);
                        }
                    }
                ),

                [Toggle.QuickAccessHistory] = (
                    Check: () =>
                    {
                        return RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowFrequent", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowRecent", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackDocs", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackProgs", "0");
                    },
                    Apply: (state, _) =>
                    {
                        if (state)
                        {
                            RegistryHelper.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowFrequent");
                            RegistryHelper.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowRecent");
                            RegistryHelper.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackDocs");
                            RegistryHelper.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackProgs");
                        }
                        else
                        {
                            RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowFrequent", 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowRecent", 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackDocs", 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackProgs", 0, RegistryValueKind.DWord);
                        }
                    }
                ),

                [Toggle.RemovableMediaAutoplay] = (
                    Check: () => RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers", "DisableAutoplay", "1"),
                    Apply: (state, _) => RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers", "DisableAutoplay", state ? 0 : 1, RegistryValueKind.DWord)
                ),

                [Toggle.PowerScheme] = (
                    Check: () =>
                    {
                        return !RegistryHelper.GetValue($@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes\{_currentPowerGuid}", "Description", string.Empty).Contains("-18") &&
                        !RegistryHelper.GetValue($@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes\{_currentPowerGuid}", "FriendlyName", string.Empty).Contains("-19");
                    },
                    Apply: (state, _) => SetPowercfg(state)
                ),

                [Toggle.BluetoothFunction] = (
                    Check: () => BluetoothManager.IsEnabled,
                    Apply: (state, _) => BluetoothManager.SetState(state)
                ),

                [Toggle.WindowsFirewall] = (
                    Check: () => RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\mpssvc", "Start", "4"),
                    Apply: (state, _) =>
                    {
                        RegistryHelper.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\mpssvc", "Start", state ? 2 : 4, RegistryValueKind.DWord);
                        CommandExecutor.RunCommand($"/c netsh advfirewall set allprofiles state {(state ? "on" : "off")}");
                        if (HardwareData.OS.Build.CompareTo(22621.521m) >= 0)
                        {
                            RegistryHelper.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\wtd", "Start", state ? 2 : 4, RegistryValueKind.DWord);
                        }
                    }
                ),

                [Toggle.GameMode] = (
                    Check: () =>
                    {
                        return RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\GameBar", "AutoGameModeEnabled", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\GameBar", "AllowAutoGameMode", "0");
                    },
                    Apply: (state, _) =>
                    {
                        RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\GameBar", "AutoGameModeEnabled", state ? 1 : 0, RegistryValueKind.DWord);
                        RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\GameBar", "AllowAutoGameMode", state ? 1 : 0, RegistryValueKind.DWord);
                    }
                ),

                [Toggle.GameBar] = (
                    Check: () =>
                    {
                        return RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\GameBar", "UseNexusForGameBarEnabled", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\System\GameConfigStore", "GameDVR_Enabled", "0");
                    },
                    Apply: (state, _) =>
                    {
                        RegistryHelper.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", state ? 1 : 0, RegistryValueKind.DWord);
                        RegistryHelper.Write(Registry.CurrentUser, @"System\GameConfigStore", "GameDVR_Enabled", state ? 1 : 0, RegistryValueKind.DWord);
                        RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\GameBar", "UseNexusForGameBarEnabled", state ? 1 : 0, RegistryValueKind.DWord);
                    }
                ),

                [Toggle.BackgroundApps] = (
                    Check: () =>
                    {
                        return RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications", "GlobalUserDisabled", "1") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Search", "BackgroundAppGlobalToggle", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsRunInBackground", "2");
                    },
                    Apply: (state, _) =>
                    {
                        if (state)
                        {
                            RegistryHelper.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications", "GlobalUserDisabled");
                            RegistryHelper.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "BackgroundAppGlobalToggle");
                            RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy");
                        }
                        else
                        {
                            RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications", "GlobalUserDisabled", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "BackgroundAppGlobalToggle", 0, RegistryValueKind.DWord);
                            RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy");
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsRunInBackground", 2, RegistryValueKind.DWord);
                        }
                    }
                ),

                [Toggle.ReservedStorage] = (
                    Check: () =>
                    {
                        return RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\ReserveManager", "MiscPolicyInfo", "2") ||
                        RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\ReserveManager", "PassedPolicy", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\ReserveManager", "ShippedWithReserves", "0");
                    },
                    Apply: (state, _) =>
                    {
                        if (state)
                        {
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ReserveManager", "MiscPolicyInfo");
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ReserveManager", "PassedPolicy", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ReserveManager", "ShippedWithReserves", 1, RegistryValueKind.DWord);
                        }
                        else
                        {
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ReserveManager", "MiscPolicyInfo", 2, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ReserveManager", "PassedPolicy", 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ReserveManager", "ShippedWithReserves", 0, RegistryValueKind.DWord);
                        }
                    }
                ),

                [Toggle.DynamicTickHpet] = (
                    Check: () => _isTickState,
                    Apply: (state, _) =>
                    {
                        _isTickState = state;
                        CommandExecutor.RunCommand(state ? $"{PathTargets.Executable.BcdEdit} /deletevalue disabledynamictick; {PathTargets.Executable.BcdEdit} /deletevalue useplatformclock" : $"{PathTargets.Executable.BcdEdit} /set disabledynamictick yes; {PathTargets.Executable.BcdEdit} /set useplatformclock false", true);
                    }
                ),


                [Toggle.HealthCheck] = (
                    Check: () =>
                    {
                        return RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PCHC", "PreviousUninstall", "1", true) ||
                        RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PCHealthCheck", "installed", "1", true);
                    },
                    Apply: (state, _) =>
                    {
                        if (state)
                        {
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\PCHC", "PreviousUninstall", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\PCHealthCheck", "installed", 1, RegistryValueKind.DWord);
                        }
                        else
                        {
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\PCHC", "PreviousUninstall");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\PCHealthCheck", "installed");
                        }
                    }
                ),

                [Toggle.InsiderTasks] = (
                    Check: () => IsTaskEnabled(winInsiderTasks),
                    Apply: (state, _) => SetTaskState(state, winInsiderTasks)
                ),

                [Toggle.SystemDriveDefrag] = (
                    Check: () =>
                    {
                        return !IsTaskEnabled(defragTask) || RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Dfrg\BootOptimizeFunction", "Enable", "N") ||
                        RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\services\defragsvc", "Start", "2");
                    },
                    Apply: (state, _) =>
                    {
                        SetTaskState(true, defragTask);
                        RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Dfrg\BootOptimizeFunction", "Enable", state ? "Y" : "N", RegistryValueKind.String);
                        RegistryHelper.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\services\defragsvc", "Start", 2, RegistryValueKind.DWord);
                    }
                ),

                [Toggle.PauseWindowsUpdates] = (
                    Check: () =>
                    {
                        return DateTime.TryParse(RegistryHelper.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "PauseUpdatesStartTime", string.Empty), null, System.Globalization.DateTimeStyles.RoundtripKind, out var start) &&
                        DateTime.TryParse(RegistryHelper.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "PauseUpdatesExpiryTime", string.Empty), null, System.Globalization.DateTimeStyles.RoundtripKind, out var end) &&
                        (end - start).TotalDays >= 3650;
                    },
                    Apply: (state, _) =>
                    {
                        if (!state)
                        {
                            DateTime now = DateTime.UtcNow;
                            string start = now.ToString("yyyy-MM-ddTHH:mm:ssZ");
                            string end = now.AddYears(10).ToString("yyyy-MM-ddTHH:mm:ssZ");

                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "NoAutoUpdate", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "AUOptions", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "PauseFeatureUpdatesStartTime", start, RegistryValueKind.String);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "PauseFeatureUpdatesEndTime", end, RegistryValueKind.String);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "PauseQualityUpdatesStartTime", start, RegistryValueKind.String);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "PauseQualityUpdatesEndTime", end, RegistryValueKind.String);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "PauseUpdatesStartTime", start, RegistryValueKind.String);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "PauseUpdatesExpiryTime", end, RegistryValueKind.String);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\Settings", "PausedFeatureStatus", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\Settings", "PausedQualityStatus", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\Settings", "PausedFeatureDate", start, RegistryValueKind.String);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\Settings", "PausedQualityDate", start, RegistryValueKind.String);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState", "IsDeferralIsActive", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState", "PolicySources", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState", "QualityUpdatesPaused", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState", "QualityUpdatePausePeriodInDays", 447, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState", "FeatureUpdatesPaused", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState", "FeatureUpdatePausePeriodInDays", 447, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState", "PauseFeatureUpdatesStartTime", start, RegistryValueKind.String);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState", "PauseFeatureUpdatesEndTime", end, RegistryValueKind.String);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState", "PauseQualityUpdatesStartTime", start, RegistryValueKind.String);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState", "PauseQualityUpdatesEndTime", end, RegistryValueKind.String);
                            CommandExecutor.RunCommand("/c gpupdate /force");
                        }
                        else
                        {
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "NoAutoUpdate");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "AUOptions");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "PauseFeatureUpdatesStartTime");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "PauseFeatureUpdatesEndTime");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "PauseQualityUpdatesStartTime");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "PauseQualityUpdatesEndTime");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "PauseUpdatesStartTime");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "PauseUpdatesExpiryTime");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\Settings", "PausedFeatureStatus");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\Settings", "PausedQualityStatus");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\Settings", "PausedFeatureDate");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\Settings", "PausedQualityDate");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState", "IsDeferralIsActive");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState", "PolicySources");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState", "QualityUpdatesPaused");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState", "QualityUpdatePausePeriodInDays");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState", "FeatureUpdatesPaused");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState", "FeatureUpdatePausePeriodInDays");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState", "PauseFeatureUpdatesStartTime");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState", "PauseFeatureUpdatesEndTime");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState", "PauseQualityUpdatesStartTime");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState", "PauseQualityUpdatesEndTime");
                        }
                    }
                ),

                [Toggle.MultiPlaneOverlay] = (
                    Check: () =>
                    {
                        return RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\Dwm", "OverlayTestMode", "5") ||
                        (HardwareData.OS.Build >= 26200m && RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\Dwm", "EnableOverlay", "0"));
                    },
                    Apply: (state, _) =>
                    {
                        bool isSupportedBuild = HardwareData.OS.Build >= 26200;

                        if (state)
                        {
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\Dwm", "OverlayTestMode");
                            if (isSupportedBuild)
                            {
                                RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\Dwm", "EnableOverlay");
                            }
                        }
                        else
                        {
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\Dwm", "OverlayTestMode", 5, RegistryValueKind.DWord);
                            if (isSupportedBuild)
                            {
                                RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\Dwm", "EnableOverlay", 0, RegistryValueKind.DWord);
                            }
                        }
                    }
                ),
            };
        }

        internal static void ViewNetshState()
        {
            try
            {
                string getStateNetsh = CommandExecutor.GetCommandOutput("/c chcp 65001 & netsh int teredo show state & netsh int ipv6 isatap show state & netsh int isatap show state & netsh int ipv6 6to4 show state", false).GetAwaiter().GetResult();
                _isNetshState = getStateNetsh.Contains("default") || getStateNetsh.Contains("enabled");
            }
            catch { _isNetshState = false; }
        }

        internal static void ViewConfigTick()
        {
            try
            {
                string output = CommandExecutor.GetCommandOutput(PathTargets.Executable.BcdEdit).GetAwaiter().GetResult();
                _isTickState = !Regex.IsMatch(output, @"(?is)(?=.*\bdisabledynamictick\s+(yes|true))(?=.*\buseplatformclock\s+(no|false))", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);
            }
            catch { _isTickState = false; }
        }

        internal void CheckAll()
        {
            foreach (var tweak in _sliderMappings)
            {
                _controlWriter[tweak.Key] = tweak.Value.Check();
            }

            foreach (var tweak in _toggleMappings)
            {
                _controlWriter[tweak.Key] = tweak.Value.Check();
            }
        }

        [DllImport("user32.dll")]
        private static extern bool SystemParametersInfo(uint _uiAction, uint _uiParam, uint _pvParam, uint _fWinIni);

        internal void Apply(string controlName, uint value)
        {
            INIManager.TempWrite(INIManager.TempTweaksSys, controlName, value);

            if (Enum.TryParse<Slider>(controlName, out var sliderKey) && _sliderMappings.TryGetValue(sliderKey, out var sliderAction))
            {
                Task.Run(() => sliderAction.Apply(value));
            }
        }

        [DllImport("user32.dll")]
        private static extern bool SystemParametersInfo(uint _uiAction, uint _uiParam, uint[] _pvParam, uint _fWinIni);

        internal void Apply(string controlName, bool state, bool canShowWindow = true)
        {
            INIManager.TempWrite(INIManager.TempTweaksSys, controlName, state);

            if (Enum.TryParse<Toggle>(controlName, out var tglKey) && _toggleMappings.TryGetValue(tglKey, out var tglAction))
            {
                if (tglKey == Toggle.WindowsDefender)
                {
                    tglAction.Apply(state, canShowWindow);
                }
                else
                {
                    Task.Run(() => tglAction.Apply(state, canShowWindow));
                }
            }
        }

        private static void SetPowercfg(bool state)
        {
            Task.Run(async () =>
            {
                Process _powercfg = new Process()
                {
                    StartInfo = {
                        FileName = PathTargets.Executable.PowerCfg,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = true,
                        Verb = "runas",
                        CreateNoWindow = true
                    },
                };

                string searchScheme = default, unlockFrequency = @"-attributes SUB_PROCESSOR 75b0ae3f-bce0-45a7-8c89-c9611c25e100 -ATTRIB_HIDE";

                try
                {
                    if (state)
                    {
                        string activeScheme = @"Microsoft:PowerPlan\\{" + RegistryHelper.GetValue($@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes\{searchScheme}", "ActivePowerScheme", string.Empty) + "}";
                        string selectedScheme = string.Empty, backupScheme = string.Empty;

                        using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\cimv2\power", "SELECT InstanceID FROM Win32_PowerPlan WHERE InstanceID !='" + activeScheme + "'"))
                        {
                            foreach (ManagementObject managementObj in searcher.Get().Cast<ManagementObject>())
                            {
                                using (managementObj)
                                {
                                    searchScheme = Regex.Match(Convert.ToString(managementObj["InstanceID"]), @"\{([^)]*)\}").Groups[1].Value;

                                    if (!RegistryHelper.GetValue($@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes\{searchScheme}", "Description", string.Empty).Contains("-10") &&
                                        !RegistryHelper.GetValue($@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes\{searchScheme}", "FriendlyName", string.Empty).Contains("-11"))
                                    {
                                        selectedScheme = searchScheme;
                                        break;
                                    }

                                    backupScheme ??= searchScheme;
                                }
                            }
                        }


                        selectedScheme ??= backupScheme;

                        if (!string.IsNullOrEmpty(selectedScheme))
                        {
                            _currentPowerGuid = selectedScheme;

                            using (_powercfg)
                            {
                                _powercfg.StartInfo.Arguments = $"/setactive {selectedScheme}";
                                _powercfg.Start();
                            }
                        }
                    }
                    else
                    {
                        using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\cimv2\power", "SELECT InstanceID FROM Win32_PowerPlan WHERE IsActive=false"))
                        {
                            foreach (ManagementObject managementObj in searcher.Get().Cast<ManagementObject>())
                            {
                                using (managementObj)
                                {
                                    searchScheme = Regex.Match(Convert.ToString(managementObj["InstanceID"]), @"\{([^)]*)\}").Groups[1].Value;

                                    if (RegistryHelper.GetValue($@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes\{searchScheme}", "Description", string.Empty).Contains("-18") &&
                                        RegistryHelper.GetValue($@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes\{searchScheme}", "FriendlyName", string.Empty).Contains("-19"))
                                    {
                                        _currentPowerGuid = searchScheme;

                                        using (_powercfg)
                                        {
                                            _powercfg.StartInfo.Arguments = $"/setactive {searchScheme}";
                                            _powercfg.Start();

                                            _powercfg.StartInfo.Arguments = unlockFrequency;
                                            _powercfg.Start();
                                        }

                                        break;
                                    }
                                    else
                                    {
                                        searchScheme = string.Empty;
                                    }
                                }
                            }
                        }


                        if (string.IsNullOrEmpty(searchScheme))
                        {
                            string _guid = Guid.NewGuid().ToString("D");

                            using (_powercfg)
                            {
                                _powercfg.StartInfo.Arguments = $"/duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61 {_guid}";
                                _powercfg.Start();

                                _powercfg.StartInfo.Arguments = $"/setactive {_guid}";
                                _powercfg.Start();

                                _powercfg.StartInfo.Arguments = unlockFrequency;
                                _powercfg.Start();

                                foreach (string type in new[] { "monitor-timeout", "standby-timeout" })
                                {
                                    foreach (string source in new[] { "ac", "dc" })
                                    {
                                        _powercfg.StartInfo.Arguments = $"/change {type}-{source} 0";
                                        _powercfg.Start();
                                    }
                                }
                            }

                            _currentPowerGuid = _guid;
                        }
                    }
                }
                catch (Exception ex) { ErrorLogger.LogDebug(ex); }
            });
        }
    }
}
