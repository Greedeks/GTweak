using GTweak.Utilities.Configuration;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Managers;
using GTweak.Utilities.Tweaks.DefenderManager;
using GTweak.View;
using GTweak.Windows;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

    internal sealed class SystemTweaks : FirewallManager
    {
        private static bool _isNetshState = false, _isBluetoothStatus = false, _isTickState = false;
        private readonly string _activeGuid = RegistryHelp.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes", "ActivePowerScheme", string.Empty);

        internal void AnalyzeAndUpdate(SystemView systemV)
        {
            systemV.Slider1.Value = RegistryHelp.GetValue<double>(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseSensitivity", 10);

            systemV.Slider2.Value = RegistryHelp.GetValue<double>(@"HKEY_CURRENT_USER\Control Panel\Keyboard", "KeyboardDelay", 1);

            systemV.Slider3.Value = RegistryHelp.GetValue<double>(@"HKEY_CURRENT_USER\Control Panel\Keyboard", "KeyboardSpeed", 31);

            systemV.TglButton1.StateNA =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseSpeed", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseThreshold1", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseThreshold2", "0");

            systemV.TglButton2.StateNA =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Accessibility\StickyKeys", "Flags", "26") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Accessibility\Keyboard Response", "Flags", "26");

            systemV.TglButton3.StateNA =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\7516b95f-f776-4464-8c53-06167f40cc99\8EC4B3A5-6868-48c2-BE75-4F3044BE88A7", "Attributes", "2");

            systemV.TglButton4.StateNA =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Power", "HiberbootEnabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power", "HibernateEnabled", "0");

            systemV.TglButton5.StateNA =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "AutoEndTasks", "1");

            systemV.TglButton6.StateNA =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\WindowsStore", "AutoDownload", "2");

            try
            {
                using RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e96c-e325-11ce-bfc1-08002be10318}");
                if (regKey != null)
                {
                    foreach (var subKeyName in regKey.GetSubKeyNames())
                    {
                        if (subKeyName == "Properties")
                            continue;

                        using RegistryKey subKey = regKey.OpenSubKey(subKeyName);
                        if (subKey != null)
                        {
                            if (subKey.GetValue("DriverDesc") is string driverDesc && driverDesc.Equals("Realtek High Definition Audio", StringComparison.OrdinalIgnoreCase))
                            {
                                using RegistryKey powerSettingsKey = subKey.OpenSubKey("PowerSettings");
                                if (powerSettingsKey != null)
                                {
                                    if (!(powerSettingsKey.GetValue("ConservationIdleTime") is byte[] conservationIdleTime) || !(powerSettingsKey.GetValue("IdlePowerState") is byte[] idlePowerState) || !(powerSettingsKey.GetValue("PerformanceIdleTime") is byte[] performanceIdleTime))
                                        systemV.TglButton7.StateNA = false;
                                    else
                                        systemV.TglButton7.StateNA = conservationIdleTime?[0].ToString() != "255" || idlePowerState?[0].ToString() != "0" || performanceIdleTime?[0].ToString() != "255";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { ErrorLogging.LogDebug(ex); }

            systemV.TglButton8.StateNA = File.Exists(Path.Combine(PathLocator.Folders.SystemDrive, @"Windows\System32\smartscreen.exe"));

            systemV.TglButton9.StateNA =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "PromptOnSecureDesktop", "0") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableLUA", "0") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableInstallerDetection", "0") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableSecureUIAPaths", "0") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "FilterAdministratorToken", "0") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableVirtualization", "0") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "ConsentPromptBehaviorAdmin", "0");

            systemV.TglButton10.StateNA =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Notifications\Settings\Windows.SystemToast.SecurityAndMaintenance", "Enabled", "0");

            systemV.TglButton11.StateNA =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Internet Explorer\Security", "DisableSecuritySettingsCheck", "1") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings\Zones\3", "1806", "0");

            systemV.TglButton12.StateNA = IsTaskEnabled(memoryDiagTasks);

            systemV.TglButton13.StateNA = _isNetshState;

            systemV.TglButton14.StateNA =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "LargeSystemCache", "1");

            systemV.TglButton15.StateNA =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Serialize", "Startupdelayinmsec", "0");

            systemV.TglButton16.StateNA =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowFrequent", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowRecent", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackDocs", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackProgs", "0");

            systemV.TglButton17.StateNA =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers", "DisableAutoplay", "1");

            systemV.TglButton18.StateNA = !RegistryHelp.GetValue($@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes\{_activeGuid}", "Description", string.Empty).Contains("-18") &&
                                          !RegistryHelp.GetValue($@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes\{_activeGuid}", "FriendlyName", string.Empty).Contains("-19");

            systemV.TglButton19.StateNA = _isBluetoothStatus;

            systemV.TglButton20.StateNA =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\mpssvc", "Start", "4");

            systemV.TglButton21.StateNA =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\GameBar", "AutoGameModeEnabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\GameBar", "AllowAutoGameMode", "0");

            systemV.TglButton22.StateNA =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\GameBar", "UseNexusForGameBarEnabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\System\GameConfigStore", "GameDVR_Enabled", "0");

            systemV.TglButton23.StateNA =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications", "GlobalUserDisabled", "1") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Search", "BackgroundAppGlobalToggle", "0") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsRunInBackground", "2");

            systemV.TglButton24.StateNA =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\ReserveManager", "MiscPolicyInfo", "2") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\ReserveManager", "PassedPolicy", "0") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\ReserveManager", "ShippedWithReserves", "0");

            systemV.TglButton25.StateNA = _isTickState;

            systemV.TglButton26.StateNA =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PCHC", "PreviousUninstall", "1", false) ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PCHealthCheck", "installed", "1", false);

            systemV.TglButton27.StateNA = IsTaskEnabled(winInsiderTasks);
        }


        internal void ViewBluetoothStatus()
        {
            try
            {
                using var managementObj = new ManagementObjectSearcher("SELECT DeviceID FROM Win32_PnPEntity WHERE Service='BthLEEnum'").Get();
                _isBluetoothStatus = managementObj.Cast<ManagementObject>().Any();
            }
            catch { _isBluetoothStatus = false; }
        }

        internal void ViewNetshState()
        {
            string getStateNetsh = CommandExecutor.GetCommandOutput("/c chcp 65001 & netsh int teredo show state & netsh int ipv6 isatap show state & netsh int isatap show state & netsh int ipv6 6to4 show state", false).Result;
            _isNetshState = getStateNetsh.Contains("default") || getStateNetsh.Contains("enabled");
        }

        internal void ViewConfigTick()
        {
            string output = CommandExecutor.GetCommandOutput(PathLocator.Executable.BcdEdit).Result;
            _isTickState = !Regex.IsMatch(output, @"(?is)(?=.*\bdisabledynamictick\s+(yes|true))(?=.*\buseplatformclock\s+(no|false))", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);
        }

        [DllImport("user32.dll")]
        private static extern bool SystemParametersInfo(uint _uiAction, uint _uiParam, uint _pvParam, uint _fWinIni);
        internal void ApplyTweaksSlider(string tweak, uint value)
        {
            INIManager.TempWrite(INIManager.TempTweaksSys, tweak, value);

            switch (tweak)
            {
                case "Slider1":
                    SystemParametersInfo(UIAction.SPI_SETMOUSESPEED, value, value, 2);
                    RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Mouse", "MouseSensitivity", value, RegistryValueKind.String);
                    break;
                case "Slider2":
                    SystemParametersInfo(UIAction.SPI_SETKEYBOARDDELAY, value, value, 2);
                    RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Keyboard", "KeyboardDelay", value, RegistryValueKind.String);
                    break;
                case "Slider3":
                    SystemParametersInfo(UIAction.SPI_SETKEYBOARDSPEED, value, value, 2);
                    RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Keyboard", "KeyboardSpeed", value, RegistryValueKind.String);
                    break;
            }
        }


        [DllImport("user32.dll")]
        private static extern bool SystemParametersInfo(uint _uiAction, uint _uiParam, uint[] _pvParam, uint _fWinIni);
        internal async void ApplyTweaks(string tweak, bool isDisabled, bool canShowWindow = true)
        {
            INIManager.TempWrite(INIManager.TempTweaksSys, tweak, isDisabled);

            switch (tweak)
            {
                case "TglButton1":
                    SystemParametersInfo(UIAction.SPI_SETMOUSE, 0, isDisabled ? new uint[3] : new uint[3] { 1, 6, 10 }, 2);
                    RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Mouse", "MouseSpeed", isDisabled ? "0" : "1", RegistryValueKind.String);
                    RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Mouse", "MouseThreshold1", isDisabled ? "0" : "6", RegistryValueKind.String);
                    RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Mouse", "MouseThreshold2", isDisabled ? "0" : "10", RegistryValueKind.String);
                    break;
                case "TglButton2":
                    RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Accessibility\StickyKeys", "Flags", isDisabled ? "26" : "507", RegistryValueKind.String);
                    RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Accessibility\Keyboard Response", "Flags", isDisabled ? "26" : "58", RegistryValueKind.String);
                    break;
                case "TglButton3":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Power\PowerSettings\7516b95f-f776-4464-8c53-06167f40cc99\8EC4B3A5-6868-48c2-BE75-4F3044BE88A7", "Attributes", isDisabled ? 2 : 1, RegistryValueKind.DWord);
                    break;
                case "TglButton4":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Power", "HiberbootEnabled", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Power", "HibernateEnabled", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    CommandExecutor.RunCommand(@$"/c powercfg.exe -h {(isDisabled ? "off" : "on")}");
                    break;
                case "TglButton5":
                    if (isDisabled)
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Desktop", "AutoEndTasks", "1", RegistryValueKind.String);
                    else
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"Control Panel\Desktop", "AutoEndTasks");
                    break;
                case "TglButton6":
                    if (isDisabled)
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\WindowsStore", "AutoDownload", 2, RegistryValueKind.DWord);
                    else
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\WindowsStore", "AutoDownload");
                    break;
                case "TglButton7":
                    try
                    {
                        using RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e96c-e325-11ce-bfc1-08002be10318}");
                        if (regKey != null)
                        {
                            foreach (var subKeyName in regKey.GetSubKeyNames())
                            {
                                using RegistryKey subKey = regKey.OpenSubKey(subKeyName);
                                if (subKey != null)
                                {
                                    if (subKey.GetValue("DriverDesc") is string driverDesc && driverDesc.Equals("Realtek High Definition Audio", StringComparison.OrdinalIgnoreCase))
                                    {
                                        RegistryHelp.Write(Registry.LocalMachine, $@"{@"SYSTEM\CurrentControlSet\Control\Class\{4d36e96c-e325-11ce-bfc1-08002be10318}"}\{subKeyName}\PowerSettings", "ConservationIdleTime", isDisabled ? new byte[] { 0xFF, 0xFF, 0xFF, 0xFF } : new byte[] { 0x0a, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                                        RegistryHelp.Write(Registry.LocalMachine, $@"{@"SYSTEM\CurrentControlSet\Control\Class\{4d36e96c-e325-11ce-bfc1-08002be10318}"}\{subKeyName}\PowerSettings", "IdlePowerState", isDisabled ? Encoding.Unicode.GetBytes("\0\0") : new byte[] { 0x03, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                                        RegistryHelp.Write(Registry.LocalMachine, $@"{@"SYSTEM\CurrentControlSet\Control\Class\{4d36e96c-e325-11ce-bfc1-08002be10318}"}\{subKeyName}\PowerSettings", "PerformanceIdleTime", isDisabled ? new byte[] { 0xFF, 0xFF, 0xFF, 0xFF } : new byte[] { 0x0a, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                    break;
                case "TglButton8":
                    BlockWDefender(isDisabled);
                    if (canShowWindow)
                    {
                        WaitingWindow waitingWindow = new WaitingWindow();
                        waitingWindow.Show();

                        ArchiveManager.Unarchive(PathLocator.Executable.NSudo, Properties.Resources.NSudoLC);

                        BackgroundQueue backgroundQueue = new BackgroundQueue();
                        await backgroundQueue.QueueTask(delegate { new NotificationManager().Show("info", "defender_notification").Perform(); });
                        await backgroundQueue.QueueTask(delegate { WindowsDefender.SetProtectionState(isDisabled); });
                        await backgroundQueue.QueueTask(delegate { new NotificationManager(300).Show().Restart(); });

                        CommandExecutor.RunCommand($"/c timeout /t 10 && del /f \"{PathLocator.Executable.NSudo}\"");
                        waitingWindow.Close();
                    }
                    else
                        WindowsDefender.SetProtectionState(isDisabled);
                    break;
                case "TglButton9":
                    RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "ConsentPromptBehaviorAdmin", isDisabled ? 0 : 5, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableInstallerDetection", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableLUA", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableSecureUIAPaths", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableVirtualization", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "FilterAdministratorToken", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "PromptOnSecureDesktop", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    break;
                case "TglButton10":
                    RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Notifications\Settings\Windows.SystemToast.SecurityAndMaintenance", "Enabled", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    break;
                case "TglButton11":
                    if (isDisabled)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Internet Explorer\Security", "DisableSecuritySettingsCheck", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Internet Settings\Zones\3", "1806", 0, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\Internet Explorer\Security", "DisableSecuritySettingsCheck");
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Internet Settings\Zones\3", "1806");
                    }
                    break;
                case "TglButton12":
                    SetTaskState(!isDisabled, memoryDiagTasks);
                    break;
                case "TglButton13":
                    string argStateNetshSecond, argStateNetsh;
                    if (isDisabled)
                    {
                        _isNetshState = false;
                        argStateNetsh = argStateNetshSecond = @"disabled";
                    }
                    else
                    {
                        _isNetshState = true;
                        argStateNetshSecond = @"enabled";
                        argStateNetsh = @"default";
                    }

                    CommandExecutor.RunCommand($"/c netsh int teredo set state {argStateNetsh} & netsh int ipv6 6to4 set state state = {argStateNetsh} undoonstop = {argStateNetsh} & netsh int ipv6 isatap set state state = {argStateNetsh} & netsh int ipv6 set privacy state = {argStateNetshSecond} & netsh int ipv6 set global randomizeidentifier = {argStateNetshSecond} & netsh int isatap set state {argStateNetsh}");
                    break;
                case "TglButton14":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "LargeSystemCache", isDisabled ? 1 : 0, RegistryValueKind.DWord);
                    break;
                case "TglButton15":
                    if (isDisabled)
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Serialize", "Startupdelayinmsec", 0, RegistryValueKind.DWord);
                    else
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Serialize", "Startupdelayinmsec");
                    break;
                case "TglButton16":
                    if (isDisabled)
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowFrequent", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowRecent", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackDocs", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackProgs", 0, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowFrequent");
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowRecent");
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackDocs");
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackProgs");
                    }
                    break;
                case "TglButton17":
                    RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers", "DisableAutoplay", isDisabled ? 1 : 0, RegistryValueKind.DWord);
                    break;
                case "TglButton18":
                    SetPowercfg(isDisabled);
                    break;
                case "TglButton19":
                    CommandExecutor.RunCommand(@"Add-Type -AssemblyName System.Runtime.WindowsRuntime
                    $asTaskGeneric = ([System.WindowsRuntimeSystemExtensions].GetMethods() | ? { $_.Name -eq 'AsTask' -and $_.GetParameters().Count -eq 1 -and $_.GetParameters()[0].ParameterType.Name -eq 'IAsyncOperation`1' })[0]
                    Function Await($WinRtTask, $ResultType) {
                        $asTask = $asTaskGeneric.MakeGenericMethod($ResultType)
                        $netTask = $asTask.Invoke($null, @($WinRtTask))
                        $netTask.Wait(-1) | Out-Null
                        $netTask.Result
                    }
                    [Windows.Devices.Radios.Radio,Windows.System.Devices,ContentType=WindowsRuntime] | Out-Null
                    [Windows.Devices.Radios.RadioAccessStatus,Windows.System.Devices,ContentType=WindowsRuntime] | Out-Null
                    Await ([Windows.Devices.Radios.Radio]::RequestAccessAsync()) ([Windows.Devices.Radios.RadioAccessStatus]) | Out-Null
                    $radios = Await ([Windows.Devices.Radios.Radio]::GetRadiosAsync()) ([System.Collections.Generic.IReadOnlyList[Windows.Devices.Radios.Radio]])
                    $bluetooth = $radios | ? { $_.Kind -eq 'Bluetooth' }
                    [Windows.Devices.Radios.RadioState,Windows.System.Devices,ContentType=WindowsRuntime] | Out-Null
                    Await ($bluetooth.SetStateAsync(" + (isDisabled ? "'off'" : "'on'") + ")) ([Windows.Devices.Radios.RadioAccessStatus]) | Out-Null", true);
                    _isBluetoothStatus = !isDisabled;
                    break;
                case "TglButton20":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\mpssvc", "Start", isDisabled ? 4 : 2, RegistryValueKind.DWord);
                    CommandExecutor.RunCommand($"/c netsh advfirewall set allprofiles state {(isDisabled ? "off" : "on")}");
                    if (SystemDiagnostics.HardwareData.OS.Build.CompareTo(22621.521m) >= 0)
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\wtd", "Start", isDisabled ? 4 : 2, RegistryValueKind.DWord);
                    break;
                case "TglButton21":
                    RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\GameBar", "AutoGameModeEnabled", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\GameBar", "AllowAutoGameMode", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    break;
                case "TglButton22":
                    RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.CurrentUser, @"System\GameConfigStore", "GameDVR_Enabled", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\GameBar", "UseNexusForGameBarEnabled", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    break;
                case "TglButton23":
                    if (isDisabled)
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications", "GlobalUserDisabled", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "BackgroundAppGlobalToggle", 0, RegistryValueKind.DWord);
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy");
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsRunInBackground", 2, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications", "GlobalUserDisabled");
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "BackgroundAppGlobalToggle");
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy");
                    }
                    break;
                case "TglButton24":
                    if (isDisabled)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ReserveManager", "MiscPolicyInfo", 2, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ReserveManager", "PassedPolicy", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ReserveManager", "ShippedWithReserves", 0, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ReserveManager", "MiscPolicyInfo");
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ReserveManager", "PassedPolicy", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ReserveManager", "ShippedWithReserves", 1, RegistryValueKind.DWord);
                    }
                    break;
                case "TglButton25":
                    if (isDisabled)
                    {
                        _isTickState = false;
                        CommandExecutor.RunCommand($@"{PathLocator.Executable.BcdEdit} /set disabledynamictick yes; {PathLocator.Executable.BcdEdit} /set useplatformclock false", true);
                    }
                    else
                    {
                        _isTickState = true;
                        CommandExecutor.RunCommand($@"{PathLocator.Executable.BcdEdit} /deletevalue disabledynamictick; {PathLocator.Executable.BcdEdit} /deletevalue useplatformclock", true);
                    }
                    break;
                case "TglButton26":
                    if (isDisabled)
                    {
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\PCHC", "PreviousUninstall");
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\PCHealthCheck", "installed");
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\PCHC", "PreviousUninstall", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\PCHealthCheck", "installed", 1, RegistryValueKind.DWord);
                    }
                    break;
                case "TglButton27":
                    SetTaskState(!isDisabled, winInsiderTasks);
                    break;
            }
        }

        private static void SetPowercfg(bool isDisabled)
        {
            Task.Run(async () =>
            {
                Process _powercfg = new Process()
                {
                    StartInfo = {
                        FileName = PathLocator.Executable.PowerCfg,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    },
                };

                string searchScheme = default, unlockFrequency = @"-attributes SUB_PROCESSOR 75b0ae3f-bce0-45a7-8c89-c9611c25e100 -ATTRIB_HIDE";

                try
                {
                    if (isDisabled)
                    {
                        foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2\power", "SELECT InstanceID FROM Win32_PowerPlan WHERE IsActive=false").Get())
                        {
                            searchScheme = Regex.Match(Convert.ToString(managementObj["InstanceID"]), @"\{([^)]*)\}").Groups[1].Value;

                            if (RegistryHelp.GetValue($@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes\{searchScheme}", "Description", string.Empty).Contains("-18") &&
                            RegistryHelp.GetValue($@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes\{searchScheme}", "FriendlyName", string.Empty).Contains("-19"))
                            {
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
                                searchScheme = string.Empty;
                        }

                        if (string.IsNullOrEmpty(searchScheme))
                        {
                            ArchiveManager.Unarchive(PathLocator.Files.PowPlan, Properties.Resources.UltPower);

                            string _guid = Guid.NewGuid().ToString("D");

                            using (_powercfg)
                            {
                                _powercfg.StartInfo.Arguments = $@"-import ""{PathLocator.Files.PowPlan}"" {_guid}";
                                _powercfg.Start();

                                await Task.Delay(5);

                                _powercfg.StartInfo.Arguments = $"/setactive {_guid}";
                                _powercfg.Start();

                                _powercfg.StartInfo.Arguments = unlockFrequency;
                                _powercfg.Start();
                            }

                            CommandExecutor.RunCommand($"/c timeout /t 10 && rd /s /q \"{PathLocator.Folders.Workspace}\"");
                        }
                    }
                    else
                    {
                        string activeScheme = @"Microsoft:PowerPlan\\{" + RegistryHelp.GetValue($@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes\{searchScheme}", "ActivePowerScheme", string.Empty) + "}";
                        string selectedScheme = string.Empty, backupScheme = string.Empty;

                        foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2\power", "SELECT InstanceID FROM Win32_PowerPlan WHERE InstanceID !='" + activeScheme + "'").Get())
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

                        selectedScheme ??= backupScheme;

                        if (!string.IsNullOrEmpty(selectedScheme))
                        {
                            using (_powercfg)
                            {
                                _powercfg.StartInfo.Arguments = $"/setactive {selectedScheme}";
                                _powercfg.Start();
                            }
                        }
                    }
                }
                catch (Exception ex) { ErrorLogging.LogDebug(ex); }
            });
        }
    }
}
