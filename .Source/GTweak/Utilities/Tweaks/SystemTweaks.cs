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
using GTweak.Utilities.Configuration;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Managers;
using GTweak.Utilities.Tweaks.DefenderManager;
using GTweak.Windows;
using Microsoft.Win32;

namespace GTweak.Utilities.Tweaks
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct UIAction
    {
        internal const uint SPI_SETMOUSESPEED = 0x0071;
        internal const uint SPI_SETKEYBOARDDELAY = 0x0017;
        internal const uint SPI_SETKEYBOARDSPEED = 0x000B;
        internal const uint SPI_SETMOUSE = 0x0004;
    };

    internal enum SystemSlider
    {
        MouseSensitivity = 1,
        KeyboardDelay,
        KeyboardSpeed
    }

    internal enum SystemToggle
    {
        PointerPrecision = 1,
        StickyKeysFilter,
        WindowsDefender,
        UserAccountControl,
        SecurityNotifications,
        StoreAutoUpdate,
        RealtekAudioDelay,
        LockScreenTimeout,
        HibernationFastStartup,
        AutoEndTasks,
        ExeLaunchWarnings,
        MemoryDiagnostics,
        NetworkProtocols,
        FileSystemCache,
        StartupDelay,
        QuickAccessHistory,
        RemovableMediaAutoplay,
        PowerScheme,
        BluetoothFunction,
        WindowsFirewall,
        GameMode,
        GameBar,
        BackgroundApps,
        ReservedStorage,
        DynamicTickHpet,
        HealthCheck,
        InsiderTasks,
        SystemDriveDefrag,
        PauseWindowsUpdates,
        MultiPlaneOverlay
    }

    internal sealed class SystemTweaks : FirewallManager
    {
        private static bool _isNetshState = false, _isTickState = false;
        private static string _currentPowerGuid = string.Empty;

        internal readonly static Dictionary<string, object> ControlStates = new Dictionary<string, object>();
        private readonly ControlWriterManager _сontrolWriter = new ControlWriterManager(ControlStates);
        private readonly Dictionary<SystemSlider, (Func<double> Check, Action<uint> Apply)> _sliderTweaks;
        private readonly Dictionary<SystemToggle, (Func<bool> Check, Action<bool, bool> Apply)> _tglTweaks;

        public SystemTweaks()
        {
            _currentPowerGuid = RegistryHelp.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes", "ActivePowerScheme", string.Empty);

            _sliderTweaks = new Dictionary<SystemSlider, (Func<double> Check, Action<uint> Apply)>
            {
                [SystemSlider.MouseSensitivity] = (
                    Check: () => RegistryHelp.GetValue<double>(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseSensitivity", 10),
                    Apply: (value) =>
                    {
                        SystemParametersInfo(UIAction.SPI_SETMOUSESPEED, value, value, 2);
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Mouse", "MouseSensitivity", value, RegistryValueKind.String);
                    }
                ),

                [SystemSlider.KeyboardDelay] = (
                    Check: () => RegistryHelp.GetValue<double>(@"HKEY_CURRENT_USER\Control Panel\Keyboard", "KeyboardDelay", 1),
                    Apply: (value) =>
                    {
                        SystemParametersInfo(UIAction.SPI_SETKEYBOARDDELAY, value, value, 2);
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Keyboard", "KeyboardDelay", value, RegistryValueKind.String);
                    }
                ),

                [SystemSlider.KeyboardSpeed] = (
                    Check: () => RegistryHelp.GetValue<double>(@"HKEY_CURRENT_USER\Control Panel\Keyboard", "KeyboardSpeed", 31),
                    Apply: (value) =>
                    {
                        SystemParametersInfo(UIAction.SPI_SETKEYBOARDSPEED, value, value, 2);
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Keyboard", "KeyboardSpeed", value, RegistryValueKind.String);
                    }
                ),
            };

            _tglTweaks = new Dictionary<SystemToggle, (Func<bool> Check, Action<bool, bool> Apply)>
            {
                [SystemToggle.PointerPrecision] = (
                    Check: () =>
                    {
                        return RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseSpeed", "0") ||
                        RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseThreshold1", "0") ||
                        RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseThreshold2", "0");
                    },
                    Apply: (state, _) =>
                    {
                        SystemParametersInfo(UIAction.SPI_SETMOUSE, 0, state ? new uint[] { 1, 6, 10 } : new uint[3], 2);
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Mouse", "MouseSpeed", state ? "1" : "0", RegistryValueKind.String);
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Mouse", "MouseThreshold1", state ? "6" : "0", RegistryValueKind.String);
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Mouse", "MouseThreshold2", state ? "10" : "0", RegistryValueKind.String);
                    }
                ),

                [SystemToggle.StickyKeysFilter] = (
                    Check: () =>
                    {
                        return RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Accessibility\StickyKeys", "Flags", "26") ||
                        RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Accessibility\Keyboard Response", "Flags", "26");
                    },
                    Apply: (state, _) =>
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Accessibility\StickyKeys", "Flags", state ? "507" : "26", RegistryValueKind.String);
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Accessibility\Keyboard Response", "Flags", state ? "58" : "26", RegistryValueKind.String);
                    }
                ),

                [SystemToggle.WindowsDefender] = (
                    Check: () => File.Exists(PathLocator.Targets.Defender.SmartScreen.Normal),
                    Apply: async (state, canShowWindow) =>
                    {
                        BlockWDefender(state);
                        ArchiveManager.Unarchive(PathLocator.Executable.NSudo, Properties.Resources.NSudoLC);
                        if (state == false)
                        {
                            ArchiveManager.Unarchive(PathLocator.Executable.DisablingWD, Properties.Resources.DisablingWD);
                        }

                        if (canShowWindow)
                        {
                            OverlayWindow overlayWindow = new OverlayWindow();
                            overlayWindow.Show();

                            BackgroundQueue backgroundQueue = new BackgroundQueue();
                            await backgroundQueue.QueueTask(delegate { NotificationManager.Show(state ? "info" : "warn", state ? "info_wd_noty" : "warn_wd_noty").Perform(); });
                            await backgroundQueue.QueueTask(delegate { WindowsDefender.SetProtectionState(state); });

                            if (state)
                            {
                                await backgroundQueue.QueueTask(delegate { NotificationManager.Show().WithDelay(300).Restart(); });
                                CommandExecutor.RunCommand($"/c timeout /t 10 && del /f \"{PathLocator.Executable.NSudo}\"");
                            }

                            overlayWindow.Close();
                        }
                        else
                        {
                            WindowsDefender.SetProtectionState(state);
                        }
                    }
                ),

                [SystemToggle.UserAccountControl] = (
                    Check: () =>
                    {
                        return RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "PromptOnSecureDesktop", "0") ||
                        RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableLUA", "0") ||
                        RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableInstallerDetection", "0") ||
                        RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableSecureUIAPaths", "0") ||
                        RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "FilterAdministratorToken", "0") ||
                        RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableVirtualization", "0") ||
                        RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "ConsentPromptBehaviorAdmin", "0");
                    },
                    Apply: (state, _) =>
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "ConsentPromptBehaviorAdmin", state ? 5 : 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableInstallerDetection", state ? 1 : 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableLUA", state ? 1 : 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableSecureUIAPaths", state ? 1 : 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableVirtualization", state ? 1 : 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "FilterAdministratorToken", state ? 1 : 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "PromptOnSecureDesktop", state ? 1 : 0, RegistryValueKind.DWord);
                    }
                ),

                [SystemToggle.SecurityNotifications] = (
                    Check: () =>
                    {
                        return RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Notifications\Settings\Windows.SystemToast.SecurityAndMaintenance", "Enabled", "0") ||
                        RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows Defender Security Center\Notifications", "DisableNotifications", "1") ||
                        RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows Defender Security Center\Notifications", "DisableNotifications", "1");
                    },
                    Apply: (state, _) =>
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Notifications\Settings\Windows.SystemToast.SecurityAndMaintenance", "Enabled", state ? 1 : 0, RegistryValueKind.DWord);
                        if (state)
                        {
                            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows Defender Security Center\Notifications", "DisableNotifications");
                            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender Security Center\Notifications", "DisableNotifications");
                        }
                        else
                        {
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows Defender Security Center\Notifications", "DisableNotifications", "1", RegistryValueKind.String);
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender Security Center\Notifications", "DisableNotifications", 1, RegistryValueKind.DWord);
                        }
                    }
                ),

                [SystemToggle.StoreAutoUpdate] = (
                    Check: () => RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\WindowsStore", "AutoDownload", "2"),
                    Apply: (state, _) =>
                    {
                        if (state)
                        {
                            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\WindowsStore", "AutoDownload");
                        }
                        else
                        {
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\WindowsStore", "AutoDownload", 2, RegistryValueKind.DWord);
                        }
                    }
                ),

                [SystemToggle.RealtekAudioDelay] = (
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
                        catch (Exception ex) { ErrorLogging.LogDebug(ex); }

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
                                            RegistryHelp.Write(Registry.LocalMachine, $@"{@"SYSTEM\CurrentControlSet\Control\Class\{4d36e96c-e325-11ce-bfc1-08002be10318}"}\{subKeyName}\PowerSettings", "ConservationIdleTime", state ? new byte[] { 0x0a, 0x00, 0x00, 0x00 } : new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, RegistryValueKind.Binary);
                                            RegistryHelp.Write(Registry.LocalMachine, $@"{@"SYSTEM\CurrentControlSet\Control\Class\{4d36e96c-e325-11ce-bfc1-08002be10318}"}\{subKeyName}\PowerSettings", "IdlePowerState", state ? new byte[] { 0x03, 0x00, 0x00, 0x00 } : Encoding.Unicode.GetBytes("\0\0"), RegistryValueKind.Binary);
                                            RegistryHelp.Write(Registry.LocalMachine, $@"{@"SYSTEM\CurrentControlSet\Control\Class\{4d36e96c-e325-11ce-bfc1-08002be10318}"}\{subKeyName}\PowerSettings", "PerformanceIdleTime", state ? new byte[] { 0x0a, 0x00, 0x00, 0x00 } : new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, RegistryValueKind.Binary);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                    }
                ),

                [SystemToggle.LockScreenTimeout] = (
                    Check: () => RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\7516b95f-f776-4464-8c53-06167f40cc99\8EC4B3A5-6868-48c2-BE75-4F3044BE88A7", "Attributes", "2"),
                    Apply: (state, _) => RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Power\PowerSettings\7516b95f-f776-4464-8c53-06167f40cc99\8EC4B3A5-6868-48c2-BE75-4F3044BE88A7", "Attributes", state ? 1 : 2, RegistryValueKind.DWord)
                ),

                [SystemToggle.HibernationFastStartup] = (
                    Check: () =>
                    {
                        return RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Power", "HiberbootEnabled", "0") ||
                        RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power", "HibernateEnabled", "0") ||
                        RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FlyoutMenuSettings", "ShowHibernateOption", "0");
                    },
                    Apply: (state, _) =>
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Power", "HiberbootEnabled", state ? 1 : 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Power", "HibernateEnabled", state ? 1 : 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FlyoutMenuSettings", "ShowHibernateOption", state ? 1 : 0, RegistryValueKind.DWord);
                        CommandExecutor.RunCommand(@$"/c powercfg.exe -h {(state ? "on" : "off")}");
                    }
                ),

                [SystemToggle.AutoEndTasks] = (
                    Check: () => RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "AutoEndTasks", "1"),
                    Apply: (state, _) =>
                    {
                        if (state)
                        {
                            RegistryHelp.DeleteValue(Registry.CurrentUser, @"Control Panel\Desktop", "AutoEndTasks");
                        }
                        else
                        {
                            RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Desktop", "AutoEndTasks", "1", RegistryValueKind.String);
                        }
                    }
                ),

                [SystemToggle.ExeLaunchWarnings] = (
                    Check: () =>
                    {
                        return RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Internet Explorer\Security", "DisableSecuritySettingsCheck", "1") ||
                        RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings\Zones\3", "1806", "0");
                    },
                    Apply: (state, _) =>
                    {
                        if (state)
                        {
                            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\Internet Explorer\Security", "DisableSecuritySettingsCheck");
                            RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Internet Settings\Zones\3", "1806");
                        }
                        else
                        {
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Internet Explorer\Security", "DisableSecuritySettingsCheck", 1, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Internet Settings\Zones\3", "1806", 0, RegistryValueKind.DWord);
                        }
                    }
                ),

                [SystemToggle.MemoryDiagnostics] = (
                    Check: () => IsTaskEnabled(memoryDiagTasks),
                    Apply: (state, _) => SetTaskState(state, memoryDiagTasks)
                ),

                [SystemToggle.NetworkProtocols] = (
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

                [SystemToggle.FileSystemCache] = (
                    Check: () => RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "LargeSystemCache", "1"),
                    Apply: (state, _) => RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "LargeSystemCache", state ? 0 : 1, RegistryValueKind.DWord)
                ),

                [SystemToggle.StartupDelay] = (
                    Check: () => RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Serialize", "Startupdelayinmsec", "0"),
                    Apply: (state, _) =>
                    {
                        if (state)
                        {
                            RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Serialize", "Startupdelayinmsec");
                        }
                        else
                        {
                            RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Serialize", "Startupdelayinmsec", 0, RegistryValueKind.DWord);
                        }
                    }
                ),

                [SystemToggle.QuickAccessHistory] = (
                    Check: () =>
                    {
                        return RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowFrequent", "0") ||
                        RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowRecent", "0") ||
                        RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackDocs", "0") ||
                        RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackProgs", "0");
                    },
                    Apply: (state, _) =>
                    {
                        if (state)
                        {
                            RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowFrequent");
                            RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowRecent");
                            RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackDocs");
                            RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackProgs");
                        }
                        else
                        {
                            RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowFrequent", 0, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowRecent", 0, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackDocs", 0, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackProgs", 0, RegistryValueKind.DWord);
                        }
                    }
                ),

                [SystemToggle.RemovableMediaAutoplay] = (
                    Check: () => RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers", "DisableAutoplay", "1"),
                    Apply: (state, _) => RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers", "DisableAutoplay", state ? 0 : 1, RegistryValueKind.DWord)
                ),

                [SystemToggle.PowerScheme] = (
                    Check: () =>
                    {
                        return !RegistryHelp.GetValue($@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes\{_currentPowerGuid}", "Description", string.Empty).Contains("-18") &&
                        !RegistryHelp.GetValue($@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes\{_currentPowerGuid}", "FriendlyName", string.Empty).Contains("-19");
                    },
                    Apply: (state, _) => SetPowercfg(state)
                ),

                [SystemToggle.BluetoothFunction] = (
                    Check: () => BluetoothManager.IsEnabled,
                    Apply: (state, _) => BluetoothManager.SetState(state)
                ),

                [SystemToggle.WindowsFirewall] = (
                    Check: () => RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\mpssvc", "Start", "4"),
                    Apply: (state, _) =>
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\mpssvc", "Start", state ? 2 : 4, RegistryValueKind.DWord);
                        CommandExecutor.RunCommand($"/c netsh advfirewall set allprofiles state {(state ? "on" : "off")}");
                        if (HardwareData.OS.Build.CompareTo(22621.521m) >= 0)
                        {
                            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\wtd", "Start", state ? 2 : 4, RegistryValueKind.DWord);
                        }
                    }
                ),

                [SystemToggle.GameMode] = (
                    Check: () =>
                    {
                        return RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\GameBar", "AutoGameModeEnabled", "0") ||
                        RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\GameBar", "AllowAutoGameMode", "0");
                    },
                    Apply: (state, _) =>
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\GameBar", "AutoGameModeEnabled", state ? 1 : 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\GameBar", "AllowAutoGameMode", state ? 1 : 0, RegistryValueKind.DWord);
                    }
                ),

                [SystemToggle.GameBar] = (
                    Check: () =>
                    {
                        return RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\GameBar", "UseNexusForGameBarEnabled", "0") ||
                        RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", "0") ||
                        RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\System\GameConfigStore", "GameDVR_Enabled", "0");
                    },
                    Apply: (state, _) =>
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", state ? 1 : 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"System\GameConfigStore", "GameDVR_Enabled", state ? 1 : 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\GameBar", "UseNexusForGameBarEnabled", state ? 1 : 0, RegistryValueKind.DWord);
                    }
                ),

                [SystemToggle.BackgroundApps] = (
                    Check: () =>
                    {
                        return RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications", "GlobalUserDisabled", "1") ||
                        RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Search", "BackgroundAppGlobalToggle", "0") ||
                        RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsRunInBackground", "2");
                    },
                    Apply: (state, _) =>
                    {
                        if (state)
                        {
                            RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications", "GlobalUserDisabled");
                            RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "BackgroundAppGlobalToggle");
                            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy");
                        }
                        else
                        {
                            RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications", "GlobalUserDisabled", 1, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "BackgroundAppGlobalToggle", 0, RegistryValueKind.DWord);
                            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy");
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsRunInBackground", 2, RegistryValueKind.DWord);
                        }
                    }
                ),

                [SystemToggle.ReservedStorage] = (
                    Check: () =>
                    {
                        return RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\ReserveManager", "MiscPolicyInfo", "2") ||
                        RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\ReserveManager", "PassedPolicy", "0") ||
                        RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\ReserveManager", "ShippedWithReserves", "0");
                    },
                    Apply: (state, _) =>
                    {
                        if (state)
                        {
                            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ReserveManager", "MiscPolicyInfo");
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ReserveManager", "PassedPolicy", 1, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ReserveManager", "ShippedWithReserves", 1, RegistryValueKind.DWord);
                        }
                        else
                        {
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ReserveManager", "MiscPolicyInfo", 2, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ReserveManager", "PassedPolicy", 0, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ReserveManager", "ShippedWithReserves", 0, RegistryValueKind.DWord);
                        }
                    }
                ),

                [SystemToggle.DynamicTickHpet] = (
                    Check: () => _isTickState,
                    Apply: (state, _) =>
                    {
                        _isTickState = state;
                        CommandExecutor.RunCommand(state ? $"{PathLocator.Executable.BcdEdit} /deletevalue disabledynamictick; {PathLocator.Executable.BcdEdit} /deletevalue useplatformclock" : $"{PathLocator.Executable.BcdEdit} /set disabledynamictick yes; {PathLocator.Executable.BcdEdit} /set useplatformclock false", true);
                    }
                ),


                [SystemToggle.HealthCheck] = (
                    Check: () =>
                    {
                        return RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PCHC", "PreviousUninstall", "1", true) ||
                        RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PCHealthCheck", "installed", "1", true);
                    },
                    Apply: (state, _) =>
                    {
                        if (state)
                        {
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\PCHC", "PreviousUninstall", 1, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\PCHealthCheck", "installed", 1, RegistryValueKind.DWord);
                        }
                        else
                        {
                            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\PCHC", "PreviousUninstall");
                            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\PCHealthCheck", "installed");
                        }
                    }
                ),

                [SystemToggle.InsiderTasks] = (
                    Check: () => IsTaskEnabled(winInsiderTasks),
                    Apply: (state, _) => SetTaskState(state, winInsiderTasks)
                ),

                [SystemToggle.SystemDriveDefrag] = (
                    Check: () =>
                    {
                        return !IsTaskEnabled(defragTask) || RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Dfrg\BootOptimizeFunction", "Enable", "N") ||
                        RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\services\defragsvc", "Start", "2");
                    },
                    Apply: (state, _) =>
                    {
                        SetTaskState(true, defragTask);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Dfrg\BootOptimizeFunction", "Enable", state ? "Y" : "N", RegistryValueKind.String);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\services\defragsvc", "Start", 2, RegistryValueKind.DWord);
                    }
                ),

                [SystemToggle.PauseWindowsUpdates] = (
                    Check: () =>
                    {
                        return DateTime.TryParse(RegistryHelp.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "PauseUpdatesStartTime", string.Empty), null, System.Globalization.DateTimeStyles.RoundtripKind, out var start) &&
                        DateTime.TryParse(RegistryHelp.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "PauseUpdatesExpiryTime", string.Empty), null, System.Globalization.DateTimeStyles.RoundtripKind, out var end) &&
                        (end - start).TotalDays >= 3650;
                    },
                    Apply: (state, _) =>
                    {
                        if (!state)
                        {
                            DateTime now = DateTime.UtcNow;
                            string start = now.ToString("yyyy-MM-ddTHH:mm:ssZ");
                            string end = now.AddYears(10).ToString("yyyy-MM-ddTHH:mm:ssZ");

                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "NoAutoUpdate", 1, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "AUOptions", 1, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "PauseFeatureUpdatesStartTime", start, RegistryValueKind.String);
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "PauseFeatureUpdatesEndTime", end, RegistryValueKind.String);
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "PauseQualityUpdatesStartTime", start, RegistryValueKind.String);
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "PauseQualityUpdatesEndTime", end, RegistryValueKind.String);
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "PauseUpdatesStartTime", start, RegistryValueKind.String);
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "PauseUpdatesExpiryTime", end, RegistryValueKind.String);
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\Settings", "PausedFeatureStatus", 1, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\Settings", "PausedQualityStatus", 1, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\Settings", "PausedFeatureDate", start, RegistryValueKind.String);
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\Settings", "PausedQualityDate", start, RegistryValueKind.String);
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState", "IsDeferralIsActive", 1, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState", "PolicySources", 1, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState", "QualityUpdatesPaused", 1, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState", "QualityUpdatePausePeriodInDays", 447, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState", "FeatureUpdatesPaused", 1, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState", "FeatureUpdatePausePeriodInDays", 447, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState", "PauseFeatureUpdatesStartTime", start, RegistryValueKind.String);
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState", "PauseFeatureUpdatesEndTime", end, RegistryValueKind.String);
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState", "PauseQualityUpdatesStartTime", start, RegistryValueKind.String);
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState", "PauseQualityUpdatesEndTime", end, RegistryValueKind.String);
                            CommandExecutor.RunCommand("/c gpupdate /force");
                        }
                        else
                        {
                            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "NoAutoUpdate");
                            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "AUOptions");
                            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "PauseFeatureUpdatesStartTime");
                            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "PauseFeatureUpdatesEndTime");
                            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "PauseQualityUpdatesStartTime");
                            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "PauseQualityUpdatesEndTime");
                            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "PauseUpdatesStartTime");
                            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "PauseUpdatesExpiryTime");
                            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\Settings", "PausedFeatureStatus");
                            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\Settings", "PausedQualityStatus");
                            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\Settings", "PausedFeatureDate");
                            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\Settings", "PausedQualityDate");
                            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState", "IsDeferralIsActive");
                            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState", "PolicySources");
                            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState", "QualityUpdatesPaused");
                            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState", "QualityUpdatePausePeriodInDays");
                            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState", "FeatureUpdatesPaused");
                            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState", "FeatureUpdatePausePeriodInDays");
                            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState", "PauseFeatureUpdatesStartTime");
                            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState", "PauseFeatureUpdatesEndTime");
                            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState", "PauseQualityUpdatesStartTime");
                            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\UpdatePolicy\PolicyState", "PauseQualityUpdatesEndTime");
                        }
                    }
                ),

                [SystemToggle.MultiPlaneOverlay] = (
                    Check: () =>
                    {
                        return RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\Dwm", "OverlayTestMode", "5") ||
                        (HardwareData.OS.Build >= 26200m && RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\Dwm", "EnableOverlay", "0"));
                    },
                    Apply: (state, _) =>
                    {
                        bool isSupportedBuild = HardwareData.OS.Build >= 26200;

                        if (state)
                        {
                            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\Dwm", "OverlayTestMode");
                            if (isSupportedBuild)
                            {
                                RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\Dwm", "EnableOverlay");
                            }
                        }
                        else
                        {
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\Dwm", "OverlayTestMode", 5, RegistryValueKind.DWord);
                            if (isSupportedBuild)
                            {
                                RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\Dwm", "EnableOverlay", 0, RegistryValueKind.DWord);
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
                string output = CommandExecutor.GetCommandOutput(PathLocator.Executable.BcdEdit).GetAwaiter().GetResult();
                _isTickState = !Regex.IsMatch(output, @"(?is)(?=.*\bdisabledynamictick\s+(yes|true))(?=.*\buseplatformclock\s+(no|false))", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);
            }
            catch { _isTickState = false; }
        }

        internal void CheckAll()
        {
            foreach (var tweak in _sliderTweaks)
            {
                _сontrolWriter.Slider[(int)tweak.Key] = tweak.Value.Check();
            }

            foreach (var tweak in _tglTweaks)
            {
                _сontrolWriter.ToggleButton[(int)tweak.Key] = tweak.Value.Check();
            }
        }

        [DllImport("user32.dll")]
        private static extern bool SystemParametersInfo(uint _uiAction, uint _uiParam, uint _pvParam, uint _fWinIni);

        internal void Apply(string controlName, uint value)
        {
            INIManager.TempWrite(INIManager.TempTweaksSys, controlName, value);

            if (controlName.StartsWith("Slider") && int.TryParse(controlName.Substring(6), out int index))
            {
                if (_sliderTweaks.TryGetValue((SystemSlider)index, out var action))
                {
                    action.Apply(value);
                }
            }
        }

        [DllImport("user32.dll")]
        private static extern bool SystemParametersInfo(uint _uiAction, uint _uiParam, uint[] _pvParam, uint _fWinIni);

        internal void Apply(string controlName, bool state, bool canShowWindow = true)
        {
            INIManager.TempWrite(INIManager.TempTweaksSys, controlName, state);

            if (controlName.StartsWith("TglButton") && int.TryParse(controlName.Substring(9), out int index))
            {
                if (_tglTweaks.TryGetValue((SystemToggle)index, out var action))
                {
                    action.Apply(state, canShowWindow);
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
                        FileName = PathLocator.Executable.PowerCfg,
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
                        string activeScheme = @"Microsoft:PowerPlan\\{" + RegistryHelp.GetValue($@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes\{searchScheme}", "ActivePowerScheme", string.Empty) + "}";
                        string selectedScheme = string.Empty, backupScheme = string.Empty;

                        using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\cimv2\power", "SELECT InstanceID FROM Win32_PowerPlan WHERE InstanceID !='" + activeScheme + "'"))
                        {
                            foreach (ManagementObject managementObj in searcher.Get().Cast<ManagementObject>())
                            {
                                using (managementObj)
                                {
                                    searchScheme = Regex.Match(Convert.ToString(managementObj["InstanceID"]), @"\{([^)]*)\}").Groups[1].Value;

                                    if (!RegistryHelp.GetValue($@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes\{searchScheme}", "Description", string.Empty).Contains("-10") &&
                                        !RegistryHelp.GetValue($@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes\{searchScheme}", "FriendlyName", string.Empty).Contains("-11"))
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

                                    if (RegistryHelp.GetValue($@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes\{searchScheme}", "Description", string.Empty).Contains("-18") &&
                                        RegistryHelp.GetValue($@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes\{searchScheme}", "FriendlyName", string.Empty).Contains("-19"))
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
                catch (Exception ex) { ErrorLogging.LogDebug(ex); }
            });
        }
    }
}
