using GTweak.Utilities.Control;
using GTweak.Utilities.Helpers;
using GTweak.View;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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

    internal sealed class SystemTweaks : Firewall
    {
        internal static bool isTweakWorkingAntivirus = false;
        private static bool isNetshState = false, isBluetoothStatus = false;
        private readonly string activeGuid = RegistryHelp.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes", "ActivePowerScheme", string.Empty);

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
            catch (Exception ex) { Debug.WriteLine(ex.Message); }

            systemV.TglButton8.StateNA = File.Exists(Path.Combine(StoragePaths.SystemDisk, @"Windows\System32\smartscreen.exe")) ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows Defender Security Center\App and Browser protection", "DisallowExploitProtectionOverride", "0") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows Defender Security Center\App and Browser protection", "UILockdown", "1") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows Defender\Real-Time Protection", "DisableBehaviorMonitoring", "1") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows Defender\Real-Time Protection", "DisableRealtimeMonitoring", "1");

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

            systemV.TglButton13.StateNA = isNetshState;

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

            systemV.TglButton18.StateNA = !RegistryHelp.GetValue($@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes\{activeGuid}", "Description", string.Empty).Contains("-18") &&
                                          !RegistryHelp.GetValue($@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes\{activeGuid}", "FriendlyName", string.Empty).Contains("-19");

            systemV.TglButton19.StateNA = isBluetoothStatus;

            systemV.TglButton20.StateNA =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\mpssvc", "Start", "4");

            systemV.TglButton21.StateNA =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\GameBar", "AutoGameModeEnabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\GameBar", "AllowAutoGameMode", "0");

            systemV.TglButton22.StateNA =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\GameBar", "UseNexusForGameBarEnabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\System\GameConfigStore", "GameDVR_Enabled", "0");
        }

        internal void ViewBluetoothStatus()
        {
            try { isBluetoothStatus = new ManagementObjectSearcher("select DeviceId from Win32_PnPEntity where service='BthLEEnum'").Get().Count > 0; }
            catch { isBluetoothStatus = false; }
        }

        internal async void ViewNetshState()
        {
            string getStateNetsh = await CommandExecutor.GetCommandOutput("/c chcp 65001 & netsh int teredo show state & netsh int ipv6 isatap show state & netsh int isatap show state & netsh int ipv6 6to4 show state", false);
            isNetshState = getStateNetsh.Contains("default") || getStateNetsh.Contains("enabled");
        }

        [DllImport("user32.dll")]
        private static extern bool SystemParametersInfo(uint _uiAction, uint _uiParam, uint _pvParam, uint _fWinIni);
        internal static void ApplyTweaksSlider(string tweak, uint value)
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
        internal static void ApplyTweaks(string tweak, bool isChoose)
        {
            INIManager.TempWrite(INIManager.TempTweaksSys, tweak, isChoose);

            switch (tweak)
            {
                case "TglButton1":
                    SystemParametersInfo(UIAction.SPI_SETMOUSE, 0, isChoose ? new uint[3] : new uint[3] { 1, 6, 10 }, 2);
                    RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Mouse", "MouseSpeed", isChoose ? "0" : "1", RegistryValueKind.String);
                    RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Mouse", "MouseThreshold1", isChoose ? "0" : "6", RegistryValueKind.String);
                    RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Mouse", "MouseThreshold2", isChoose ? "0" : "10", RegistryValueKind.String);
                    break;
                case "TglButton2":
                    RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Accessibility\StickyKeys", "Flags", isChoose ? "26" : "507", RegistryValueKind.String);
                    RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Accessibility\Keyboard Response", "Flags", isChoose ? "26" : "58", RegistryValueKind.String);
                    break;
                case "TglButton3":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Power\PowerSettings\7516b95f-f776-4464-8c53-06167f40cc99\8EC4B3A5-6868-48c2-BE75-4F3044BE88A7", "Attributes", isChoose ? 2 : 1, RegistryValueKind.DWord);
                    break;
                case "TglButton4":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Power", "HiberbootEnabled", isChoose ? 0 : 1, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Power", "HibernateEnabled", isChoose ? 0 : 1, RegistryValueKind.DWord);
                    CommandExecutor.RunCommand(@$"/c powercfg.exe -h {(isChoose ? "off" : "on")}");
                    break;
                case "TglButton5":
                    if (isChoose)
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Desktop", "AutoEndTasks", "1", RegistryValueKind.String);
                    else
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"Control Panel\Desktop", "AutoEndTasks");
                    break;
                case "TglButton6":
                    if (isChoose)
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
                                        RegistryHelp.Write(Registry.LocalMachine, $@"{@"SYSTEM\CurrentControlSet\Control\Class\{4d36e96c-e325-11ce-bfc1-08002be10318}"}\{subKeyName}\PowerSettings", "ConservationIdleTime", isChoose ? new byte[] { 0xFF, 0xFF, 0xFF, 0xFF } : new byte[] { 0x0a, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                                        RegistryHelp.Write(Registry.LocalMachine, $@"{@"SYSTEM\CurrentControlSet\Control\Class\{4d36e96c-e325-11ce-bfc1-08002be10318}"}\{subKeyName}\PowerSettings", "IdlePowerState", isChoose ? Encoding.Unicode.GetBytes("\0\0") : new byte[] { 0x03, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                                        RegistryHelp.Write(Registry.LocalMachine, $@"{@"SYSTEM\CurrentControlSet\Control\Class\{4d36e96c-e325-11ce-bfc1-08002be10318}"}\{subKeyName}\PowerSettings", "PerformanceIdleTime", isChoose ? new byte[] { 0xFF, 0xFF, 0xFF, 0xFF } : new byte[] { 0x0a, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex) { Debug.WriteLine(ex.Message); }
                    break;
                case "TglButton8":
                    BackgroundWorker backgroundWorker = new BackgroundWorker();
                    backgroundWorker.DoWork += delegate
                    {
                        if (isChoose)
                            new WindowsDefender().Disable();
                        else
                            new WindowsDefender().Enable();
                    };
                    backgroundWorker.RunWorkerCompleted += delegate { isTweakWorkingAntivirus = false; new ViewNotification(300).Show("restart"); };
                    backgroundWorker.RunWorkerAsync();
                    BlockWDefender(isChoose);
                    break;
                case "TglButton9":
                    RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "ConsentPromptBehaviorAdmin", isChoose ? 0 : 5, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableInstallerDetection", isChoose ? 0 : 1, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableLUA", isChoose ? 0 : 1, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableSecureUIAPaths", isChoose ? 0 : 1, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableVirtualization", isChoose ? 0 : 1, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "FilterAdministratorToken", isChoose ? 0 : 1, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "PromptOnSecureDesktop", isChoose ? 0 : 1, RegistryValueKind.DWord);
                    break;
                case "TglButton10":
                    RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Notifications\Settings\Windows.SystemToast.SecurityAndMaintenance", "Enabled", isChoose ? 0 : 1, RegistryValueKind.DWord);
                    break;
                case "TglButton11":
                    if (isChoose)
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
                    SetTaskState(!isChoose, memoryDiagTasks);
                    break;
                case "TglButton13":
                    string argStateNetsh = string.Empty, argStateNetshSecond = string.Empty;

                    if (isChoose)
                    {
                        isNetshState = false;
                        argStateNetsh = argStateNetshSecond = @"disabled";
                    }
                    else
                    {
                        isNetshState = true;
                        argStateNetshSecond = @"enabled";
                        argStateNetsh = @"default";
                    }

                    CommandExecutor.RunCommand($"/c netsh int teredo set state {argStateNetsh} & netsh int ipv6 6to4 set state state = {argStateNetsh} undoonstop = {argStateNetsh} & netsh int ipv6 isatap set state state = {argStateNetsh} & netsh int ipv6 set privacy state = {argStateNetshSecond} & netsh int ipv6 set global randomizeidentifier = {argStateNetshSecond} & netsh int isatap set state {argStateNetsh}");
                    break;
                case "TglButton14":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "LargeSystemCache", isChoose ? 1 : 0, RegistryValueKind.DWord);
                    break;
                case "TglButton15":
                    if (isChoose)
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Serialize", "Startupdelayinmsec", 0, RegistryValueKind.DWord);
                    else
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Serialize", "Startupdelayinmsec");
                    break;
                case "TglButton16":
                    if (isChoose)
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
                    RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers", "DisableAutoplay", isChoose ? 1 : 0, RegistryValueKind.DWord);
                    break;
                case "TglButton18":
                    SetPowercfg(isChoose);
                    break;
                case "TglButton19":
                    BluetoothStatusSet(isChoose ? "'off'" : "'on'");
                    isBluetoothStatus = !isChoose;
                    break;
                case "TglButton20":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\mpssvc", "Start", isChoose ? 4 : 2, RegistryValueKind.DWord);
                    CommandExecutor.RunCommand($"/c netsh advfirewall set allprofiles state {(isChoose ? "off" : "on")}");
                    break;
                case "TglButton21":
                    RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\GameBar", "AutoGameModeEnabled", isChoose ? 0 : 1, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\GameBar", "AllowAutoGameMode", isChoose ? 0 : 1, RegistryValueKind.DWord);
                    break;
                case "TglButton22":
                    RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", isChoose ? 0 : 1, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.CurrentUser, @"System\GameConfigStore", "GameDVR_Enabled", isChoose ? 0 : 1, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\GameBar", "UseNexusForGameBarEnabled", isChoose ? 0 : 1, RegistryValueKind.DWord);
                    break;
            }
        }

        private static void BluetoothStatusSet(string status)
        {
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
                    Await ($bluetooth.SetStateAsync(" + status + ")) ([Windows.Devices.Radios.RadioAccessStatus]) | Out-Null", true);
        }


        private static void SetPowercfg(bool isChoose)
        {
            Task.Run(async () =>
            {
                Process _powercfg = new Process()
                {
                    StartInfo = {
                        FileName = "powercfg",
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    },
                };

                string searchScheme = default,
                    unlockFrequency = @"-attributes SUB_PROCESSOR 75b0ae3f-bce0-45a7-8c89-c9611c25e100 -ATTRIB_HIDE",
                    pathTempFile = StoragePaths.FolderLocation + @"\UltimatePerformance.pow";

                try
                {
                    if (isChoose)
                    {
                        foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2\power", "SELECT InstanceID FROM Win32_PowerPlan WHERE IsActive=false").Get())
                        {
                            searchScheme = Convert.ToString(managementObj["InstanceID"]);
                            searchScheme = Regex.Match(searchScheme, @"\{([^)]*)\}").Groups[1].Value;

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
                            }
                        }

                        if (string.IsNullOrEmpty(searchScheme))
                        {
                            new UnarchiveManager(pathTempFile, Properties.Resources.Ultimate_Performance_pow);

                            string _guid = Guid.NewGuid().ToString("D");

                            using (_powercfg)
                            {
                                _powercfg.StartInfo.Arguments = $@"-import ""{pathTempFile}"" {_guid}";
                                _powercfg.Start();

                                await Task.Delay(5);

                                _powercfg.StartInfo.Arguments = $"/setactive {_guid}";
                                _powercfg.Start();

                                _powercfg.StartInfo.Arguments = unlockFrequency;
                                _powercfg.Start();
                            }

                            CommandExecutor.RunCommand($"/c timeout /t 10 && rd /s /q {StoragePaths.FolderLocation}");
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
                catch (Exception ex) { Debug.WriteLine(ex.Message); }
            });
        }
    }
}
