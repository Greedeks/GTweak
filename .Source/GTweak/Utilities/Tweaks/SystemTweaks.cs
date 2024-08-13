using GTweak.Utilities.Helpers;
using GTweak.View;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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

    internal sealed class SystemTweaks
    {
        private static readonly string[] schedulerTasks = new string[2] {
                @"Microsoft\Windows\MemoryDiagnostic\ProcessMemoryDiagnosticEvents",
                @"Microsoft\Windows\MemoryDiagnostic\RunFullMemoryDiagnostic"};

        private static readonly string[] netshValue = new string[4] {
                @"netsh interface teredo",
                @"netsh int ipv6 isatap",
                @"netsh interface isatap",
                @"netsh int ipv6 6to4" };

        internal static bool isTweakWorkingAntivirus = false;
        private static bool isNetshState = false, isBluetoothStatus = false;
        private static byte counTaskWorking = 0;
        private readonly string activeGuid = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes", "ActivePowerScheme", string.Empty).ToString();

        internal void ViewSystem(SystemView systemV)
        {
            if (Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseSensitivity", null) != null)
                systemV.Slider1.Value = Convert.ToDouble(Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseSensitivity", "10").ToString());


            if (Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Keyboard", "KeyboardDelay", null) != null)
                systemV.Slider2.Value = Convert.ToDouble(Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Keyboard", "KeyboardDelay", "1").ToString());


            if (Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Keyboard", "KeyboardSpeed", null) != null)
                systemV.Slider3.Value = Convert.ToDouble(Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Keyboard", "KeyboardSpeed", "31").ToString());


            if (Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseSpeed", null) == null ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseSpeed", string.Empty).ToString() != "0" ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseThreshold1", null) == null ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseThreshold1", string.Empty).ToString() != "0" ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseThreshold2", null) == null ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseThreshold2", string.Empty).ToString() != "0")
                systemV.TglButton1.StateNA = true;
            else
                systemV.TglButton1.StateNA = false;


            if (Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Accessibility\StickyKeys", "Flags", null) == null ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Accessibility\StickyKeys", "Flags", string.Empty).ToString() != "26" ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Accessibility\Keyboard Response", "Flags", null) == null ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Accessibility\Keyboard Response", "Flags", string.Empty).ToString() != "26")
                systemV.TglButton2.StateNA = true;
            else
                systemV.TglButton2.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\7516b95f-f776-4464-8c53-06167f40cc99\8EC4B3A5-6868-48c2-BE75-4F3044BE88A7", "Attributes", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\7516b95f-f776-4464-8c53-06167f40cc99\8EC4B3A5-6868-48c2-BE75-4F3044BE88A7", "Attributes", string.Empty).ToString() != "2")
                systemV.TglButton3.StateNA = true;
            else
                systemV.TglButton3.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Power", "HiberbootEnabled", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Power", "HiberbootEnabled", string.Empty).ToString() != "0" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power", "HibernateEnabled", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power", "HibernateEnabled", string.Empty).ToString() != "0")
                systemV.TglButton4.StateNA = true;
            else
                systemV.TglButton4.StateNA = false;


            if (Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "AutoEndTasks", null) == null ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "AutoEndTasks", string.Empty).ToString() != "1")
                systemV.TglButton5.StateNA = true;
            else
                systemV.TglButton5.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\WindowsStore", "AutoDownload", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\WindowsStore", "AutoDownload", string.Empty).ToString() != "2")
                systemV.TglButton6.StateNA = true;
            else
                systemV.TglButton6.StateNA = false;


            try
            {
                RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e96c-e325-11ce-bfc1-08002be10318}", true);
                foreach (string keyname in registryKey?.GetSubKeyNames())
                {
                    RegistryKey key = registryKey?.OpenSubKey(keyname);
                    if (key?.GetValue("DriverDesc")?.ToString() == "Realtek High Definition Audio")
                    {
                        RegistryKey _registrykey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e96c-e325-11ce-bfc1-08002be10318}\" + keyname + @"\PowerSettings", true);
                        byte[] _ConservationIdleTime = _registrykey.GetValue(@"ConservationIdleTime") as byte[];
                        byte[] _IdlePowerState = _registrykey.GetValue(@"IdlePowerState") as byte[];
                        byte[] _PerformanceIdleTime = _registrykey.GetValue(@"PerformanceIdleTime") as byte[];

                        if (_ConservationIdleTime?[0].ToString() != "255" || _IdlePowerState?[0].ToString() != "0" || _PerformanceIdleTime?[0].ToString() != "255")
                            systemV.TglButton7.StateNA = true;
                        else
                            systemV.TglButton7.StateNA = false;
                    }
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message.ToString()); }


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\MicrosoftEdge\PhishingFilter", "EnabledV9", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\MicrosoftEdge\PhishingFilter", "EnabledV9", string.Empty).ToString() != "0" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System", "EnableSmartScreen", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System", "EnableSmartScreen", string.Empty).ToString() != "0" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows Defender", "DisableAntiSpyware", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows Defender", "DisableAntiSpyware", string.Empty).ToString() != "1" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows Defender", "DisableAntiVirus", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows Defender", "DisableAntiVirus", string.Empty).ToString() != "1" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows Defender\SmartScreen", "ConfigureAppInstallControl", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows Defender\SmartScreen", "ConfigureAppInstallControl", string.Empty).ToString() != "Anywhere" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows Defender\SmartScreen", "ConfigureAppInstallControlEnabled", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows Defender\SmartScreen", "ConfigureAppInstallControlEnabled", string.Empty).ToString() != "0" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows Defender\Real-Time Protection", "DisableBehaviorMonitoring", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows Defender\Real-Time Protection", "DisableBehaviorMonitoring", string.Empty).ToString() != "1" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows Defender\Real-Time Protection", "DisableIOAVProtection", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows Defender\Real-Time Protection", "DisableIOAVProtection", string.Empty).ToString() != "1" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows Defender\Real-Time Protection", "DisableOnAccessProtection", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows Defender\Real-Time Protection", "DisableOnAccessProtection", string.Empty).ToString() != "1" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows Defender\Real-Time Protection", "DisableRealtimeMonitoring", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows Defender\Real-Time Protection", "DisableRealtimeMonitoring", string.Empty).ToString() != "1" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows Defender\Real-Time Protection", "DisableScanOnRealtimeEnable", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows Defender\Real-Time Protection", "DisableScanOnRealtimeEnable", string.Empty).ToString() != "1" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows Defender\Spynet", "SpyNetReporting", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows Defender\Spynet", "SpyNetReporting", string.Empty).ToString() != "0" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows Defender\Spynet", "SubmitSamplesConsent", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows Defender\Spynet", "SubmitSamplesConsent", string.Empty).ToString() != "2" ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\AppHost", "EnableWebContentEvaluation", null) == null ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\AppHost", "EnableWebContentEvaluation", string.Empty).ToString() != "0" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\SecurityHealthService", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\SecurityHealthService", "Start", string.Empty).ToString() != "4")
                systemV.TglButton8.StateNA = true;
            else
                systemV.TglButton8.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "PromptOnSecureDesktop", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "PromptOnSecureDesktop", string.Empty).ToString() != "0" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableLUA", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableLUA", string.Empty).ToString() != "0" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableInstallerDetection", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableInstallerDetection", string.Empty).ToString() != "0" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableSecureUIAPaths", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableSecureUIAPaths", string.Empty).ToString() != "0" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "FilterAdministratorToken", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "FilterAdministratorToken", string.Empty).ToString() != "0" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableVirtualization", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableVirtualization", string.Empty).ToString() != "0" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "ConsentPromptBehaviorAdmin", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "ConsentPromptBehaviorAdmin", string.Empty).ToString() != "0")
                systemV.TglButton9.StateNA = true;
            else
                systemV.TglButton9.StateNA = false;


            if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Notifications\Settings\Windows.SystemToast.SecurityAndMaintenance", "Enabled", null) == null ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Notifications\Settings\Windows.SystemToast.SecurityAndMaintenance", "Enabled", string.Empty).ToString() != "0")
                systemV.TglButton10.StateNA = true;
            else
                systemV.TglButton10.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Internet Explorer\Security", "DisableSecuritySettingsCheck", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Internet Explorer\Security", "DisableSecuritySettingsCheck", string.Empty).ToString() != "1" ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings\Zones\3", "1806", null) == null ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings\Zones\3", "1806", string.Empty).ToString() != "0")
                systemV.TglButton11.StateNA = true;
            else
                systemV.TglButton11.StateNA = false;


            if (counTaskWorking > 0)
                systemV.TglButton12.StateNA = true;
            else
                systemV.TglButton12.StateNA = false;


            if (isNetshState)
                systemV.TglButton13.StateNA = true;
            else
                systemV.TglButton13.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "LargeSystemCache", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "LargeSystemCache", string.Empty).ToString() != "1")
                systemV.TglButton14.StateNA = true;
            else
                systemV.TglButton14.StateNA = false;


            if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Serialize", "Startupdelayinmsec", null) == null ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Serialize", "Startupdelayinmsec", string.Empty).ToString() != "0")
                systemV.TglButton15.StateNA = true;
            else
                systemV.TglButton15.StateNA = false;


            if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowFrequent", null) == null ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowFrequent", string.Empty).ToString() != "0" ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowRecent", null) == null ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowRecent", string.Empty).ToString() != "0" ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackDocs", null) == null ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackDocs", string.Empty).ToString() != "0" ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackProgs", null) == null ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackProgs", string.Empty).ToString() != "0")
                systemV.TglButton16.StateNA = true;
            else
                systemV.TglButton16.StateNA = false;


            if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers", "DisableAutoplay", null) == null ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers", "DisableAutoplay", string.Empty).ToString() != "1")
                systemV.TglButton17.StateNA = true;
            else
                systemV.TglButton17.StateNA = false;


            if (!Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes\" + activeGuid + "", "Description", string.Empty).ToString().Contains("-18") && 
                !Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes\" + activeGuid + "", "FriendlyName", string.Empty).ToString().Contains("-19"))
                systemV.TglButton18.StateNA = true;
            else
                systemV.TglButton18.StateNA = false;


            if (isBluetoothStatus)
                systemV.TglButton19.StateNA = true;
            else
                systemV.TglButton19.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\mpssvc", "Start", string.Empty).ToString() != "4")
                systemV.TglButton20.StateNA = true;
            else
                systemV.TglButton20.StateNA = false;


            if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\GameBar", "AutoGameModeEnabled", null) == null ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\GameBar", "AutoGameModeEnabled", string.Empty).ToString() != "0" ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\GameBar", "AllowAutoGameMode", null) == null ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\GameBar", "AllowAutoGameMode", string.Empty).ToString() != "0")
                systemV.TglButton21.StateNA = true;
            else
                systemV.TglButton21.StateNA = false;


            if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\GameBar", "UseNexusForGameBarEnabled", null) == null ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\GameBar", "UseNexusForGameBarEnabled", string.Empty).ToString() != "0" ||
                Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", null) == null ||
                Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", string.Empty).ToString() != "0" ||
                Registry.GetValue(@"HKEY_CURRENT_USER\System\GameConfigStore", "GameDVR_Enabled", null) == null ||
                Registry.GetValue(@"HKEY_CURRENT_USER\System\GameConfigStore", "GameDVR_Enabled", string.Empty).ToString() != "0")
                systemV.TglButton22.StateNA = true;
            else
                systemV.TglButton22.StateNA = false;
        }

        internal void ViewBluetoothStatus()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT DeviceId FROM Win32_PnPEntity WHERE service='BthLEEnum'");
                isBluetoothStatus = searcher.Get().Count > 0;
            } catch { isBluetoothStatus = false; }
        }

        internal void ViewTaskState()
        {
            using Microsoft.Win32.TaskScheduler.TaskService taskService = new Microsoft.Win32.TaskScheduler.TaskService();
            foreach (string taskname in schedulerTasks)
            {
                Microsoft.Win32.TaskScheduler.Task _task = taskService.GetTask(taskname);
                if (_task != null)
                {
                    if (_task.Enabled)
                        counTaskWorking++;
                }
            }
        }

        internal void ViewNetshState()
        {
            Parallel.Invoke(() =>
            {
                try
                {
                    string getStateNetsh = default;

                    Process cmdProcess = new Process();
                    cmdProcess.StartInfo.UseShellExecute = false;
                    cmdProcess.StartInfo.RedirectStandardOutput = true;
                    cmdProcess.StartInfo.CreateNoWindow = true;
                    cmdProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    cmdProcess.StartInfo.FileName = "cmd.exe";
                    cmdProcess.StartInfo.Arguments = $"/c chcp 65001 & {netshValue[0]} show state & {netshValue[1]} show state & {netshValue[2]} show state & {netshValue[3]} show state";
                    cmdProcess.Start();

                    Parallel.Invoke(
                    () => { cmdProcess.StandardOutput.ReadLine(); },
                    () => { getStateNetsh = cmdProcess.StandardOutput.ReadToEnd(); });

                    cmdProcess.Close();
                    cmdProcess.Dispose();

                    isNetshState = getStateNetsh.Contains("default");
                }
                catch (Exception ex) { Debug.WriteLine(ex.Message.ToString()); }
            });
        }

        [DllImport("user32.dll")]
        private static extern bool SystemParametersInfo(uint _uiAction, uint _uiParam, uint _pvParam, uint _fWinIni);
        internal static void UseSystemSliders(string tweak, uint value)
        {
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
        internal static void UseSystem(string tweak, bool isChoose)
        {
            switch (tweak)
            {
                case "TglButton1":

                    uint[] _accelerate = new uint[3];

                    if (isChoose)
                    {
                        SystemParametersInfo(UIAction.SPI_SETMOUSE, 0, _accelerate, 2);
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Mouse", "MouseSpeed", 0, RegistryValueKind.String);
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Mouse", "MouseThreshold1", 0, RegistryValueKind.String);
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Mouse", "MouseThreshold2", 0, RegistryValueKind.String);
                    }
                    else
                    {
                        _accelerate = new uint[3] { 1, 6, 10 };
                        SystemParametersInfo(UIAction.SPI_SETMOUSE, 0, _accelerate, 2);
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Mouse", "MouseSpeed", 1, RegistryValueKind.String);
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Mouse", "MouseThreshold1", 6, RegistryValueKind.String);
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Mouse", "MouseThreshold2", 10, RegistryValueKind.String);
                    }
                    break;
                case "TglButton2":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Accessibility\StickyKeys", "Flags", "26", RegistryValueKind.String);
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Accessibility\Keyboard Response", "Flags", "26", RegistryValueKind.String);
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Accessibility\StickyKeys", "Flags", "507", RegistryValueKind.String);
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Accessibility\Keyboard Response", "Flags", "58", RegistryValueKind.String);
                    }
                    break;
                case "TglButton3":
                    if (isChoose)
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Power\PowerSettings\7516b95f-f776-4464-8c53-06167f40cc99\8EC4B3A5-6868-48c2-BE75-4F3044BE88A7", "Attributes", 2, RegistryValueKind.DWord);
                    else
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Power\PowerSettings\7516b95f-f776-4464-8c53-06167f40cc99\8EC4B3A5-6868-48c2-BE75-4F3044BE88A7", "Attributes", 1, RegistryValueKind.DWord);
                    break;
                case "TglButton4":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Power", "HiberbootEnabled", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Power", "HibernateEnabled", 0, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Power", "HiberbootEnabled", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Power", "HibernateEnabled", 1, RegistryValueKind.DWord);
                    }
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
                    byte[] _dataTime;
                    byte[] _dataState;

                    if (isChoose)
                    {
                        _dataTime = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };
                        _dataState = Encoding.Unicode.GetBytes("\0\0");
                    }
                    else
                    {
                        _dataTime = new byte[] { 0x0a, 0x00, 0x00, 0x00 };
                        _dataState = new byte[] { 0x03, 0x00, 0x00, 0x00 };
                    }
                    try
                    {
                        RegistryKey registrykey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e96c-e325-11ce-bfc1-08002be10318}", true);

                        foreach (string Keyname in registrykey.GetSubKeyNames())
                        {
                            RegistryKey keyView = registrykey?.OpenSubKey(Keyname);
                            if (keyView?.GetValue("DriverDesc")?.ToString() == "Realtek High Definition Audio")
                            {
                                RegistryKey keychange = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e96c-e325-11ce-bfc1-08002be10318}\" + Keyname + @"\PowerSettings", true);
                                keychange?.SetValue("ConservationIdleTime", _dataTime, RegistryValueKind.Binary);
                                keychange?.SetValue("IdlePowerState", _dataState, RegistryValueKind.Binary);
                                keychange?.SetValue("PerformanceIdleTime", _dataTime, RegistryValueKind.Binary);
                            }
                        }
                    } catch (Exception ex) { Debug.WriteLine(ex.Message.ToString()); }
                    break;
                case "TglButton8":
                    BackgroundWorker backgroundWorker = new BackgroundWorker();
                    backgroundWorker.DoWork += (s, e) => 
                    {
                        if (isChoose)
                            new WindowsDefender().Disable(); 
                        else
                            new WindowsDefender().Enable();
                    };
                    backgroundWorker.RunWorkerCompleted += (s, e) => { isTweakWorkingAntivirus = false; new ViewNotification().Show("restart"); };
                    backgroundWorker.RunWorkerAsync();
                    break;
                case "TglButton9":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "ConsentPromptBehaviorAdmin", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableInstallerDetection", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableLUA", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableSecureUIAPaths", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableVirtualization", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "FilterAdministratorToken", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "PromptOnSecureDesktop", 0, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "ConsentPromptBehaviorAdmin", 5, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableInstallerDetection", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableLUA", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableSecureUIAPaths", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableVirtualization", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "FilterAdministratorToken", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "PromptOnSecureDesktop", 1, RegistryValueKind.DWord);
                    }
                    break;
                case "TglButton10":
                    if (isChoose)
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Notifications\Settings\Windows.SystemToast.SecurityAndMaintenance", "Enabled", 0, RegistryValueKind.DWord);
                    else
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Notifications\Settings\Windows.SystemToast.SecurityAndMaintenance", "Enabled", 1, RegistryValueKind.DWord);
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
                    if (isChoose)
                    {
                        counTaskWorking = 0;
                        new Thread(() => DisablingTasks()).Start();
                        new Thread(() => DisablingTasks()).IsBackground = true;

                    }
                    else
                    {
                        counTaskWorking = 2;
                        new Thread(() => EnablingTasks()).Start();
                        new Thread(() => EnablingTasks()).IsBackground = true;
                    }
                    break;
                case "TglButton13":
                    string argumentsNetsh = string.Empty;

                    if (isChoose)
                    {
                        isNetshState = false;
                        argumentsNetsh = @"disabled";
                    }
                    else
                    {
                        isNetshState = true;
                        argumentsNetsh = @"default";
                    }

                    Parallel.Invoke(() =>
                    {
                        Process cmdProcess = new Process();
                        cmdProcess.StartInfo.UseShellExecute = false;
                        cmdProcess.StartInfo.CreateNoWindow = true;
                        cmdProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        cmdProcess.StartInfo.FileName = "cmd.exe";
                        cmdProcess.StartInfo.Arguments = string.Format(@"/c netsh interface teredo set state {0} & netsh interface ipv6 6to4 set state state={0} undoonstop={0} & netsh interface ipv6 isatap set state state={0} & netsh interface IPV6 set privacy state={0} & netsh interface IPV6 set global randomizeidentifier={0} & netsh interface isatap set state {0}", argumentsNetsh);
                        cmdProcess.Start();
                        cmdProcess.Close();
                        cmdProcess.Dispose();
                    });
                    break;
                case "TglButton14":
                    if (isChoose)
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "LargeSystemCache", 1, RegistryValueKind.DWord);
                    else
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "LargeSystemCache", 0, RegistryValueKind.DWord);
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
                    if (isChoose)
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers", "DisableAutoplay", 1, RegistryValueKind.DWord);
                    else
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers", "DisableAutoplay", 0, RegistryValueKind.DWord);
                    break;
                case "TglButton18":
                    {
                        new Thread(() => SetPowercfg(isChoose)).Start();
                        new Thread(() => SetPowercfg(isChoose)).IsBackground = true;
                    }
                    break;
                case "TglButton19":
                    if (isChoose)
                    {
                        new Thread(() => BluetoothStatusSet()).Start();
                        new Thread(() => BluetoothStatusSet()).IsBackground = true;
                        isBluetoothStatus = false;
                    }
                    else
                    {
                        new Thread(() => BluetoothStatusSet("'on'")).Start();
                        new Thread(() => BluetoothStatusSet("'on'")).IsBackground = true;
                        isBluetoothStatus = true;
                    }
                    break;
                case "TglButton20":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\mpssvc", "Start", "4", RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\StandardProfile", "EnableFirewall", "1", RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\DomainProfile", "EnableFirewall", "1", RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\mpssvc", "Start", "2", RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\StandardProfile", "EnableFirewall", "0", RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\DomainProfile", "EnableFirewall", "0", RegistryValueKind.DWord);
                    }
                    break;
                case "TglButton21":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\GameBar", "AutoGameModeEnabled", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\GameBar", "AllowAutoGameMode", 0, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\GameBar", "AutoGameModeEnabled", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\GameBar", "AllowAutoGameMode", 1, RegistryValueKind.DWord);
                    }
                    break;
                case "TglButton22":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"System\GameConfigStore", "GameDVR_Enabled", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\GameBar", "UseNexusForGameBarEnabled", 0, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"System\GameConfigStore", "GameDVR_Enabled", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\GameBar", "UseNexusForGameBarEnabled", 1, RegistryValueKind.DWord);
                    }
                    break;
            }
        }

        private static void BluetoothStatusSet(in string status = "'off'")
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = @"powershell.exe",
                Arguments = @"Add-Type -AssemblyName System.Runtime.WindowsRuntime
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
                    Await ($bluetooth.SetStateAsync(" + status + ")) ([Windows.Devices.Radios.RadioAccessStatus]) | Out-Null",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using Process process = new Process() { StartInfo = startInfo };
            process.Start();
            process.Close();
        }

        private static void EnablingTasks()
        {
            Parallel.Invoke(() =>
            {
                Microsoft.Win32.TaskScheduler.TaskService taskService = new Microsoft.Win32.TaskScheduler.TaskService();
                foreach (string taskname in schedulerTasks)
                {
                    Microsoft.Win32.TaskScheduler.Task _task = taskService.GetTask(taskname);
                    if (_task != null)
                    {
                        if (!_task.Enabled)
                        {
                            _task.Definition.Settings.Enabled = true;
                            _task.RegisterChanges();
                        }

                    }
                }
            });
        }

        private static void DisablingTasks()
        {
            Parallel.Invoke(() =>
            {
                Microsoft.Win32.TaskScheduler.TaskService taskService = new Microsoft.Win32.TaskScheduler.TaskService();
                foreach (string _taskname in schedulerTasks)
                {
                    Microsoft.Win32.TaskScheduler.Task _task = taskService.GetTask(_taskname);
                    if (_task != null)
                    {
                        if (_task.Enabled)
                        {
                            _task.Definition.Settings.Enabled = false;
                            _task.RegisterChanges();
                        }
                    }
                }
            });
        }

        private async static void SetPowercfg(bool isChoose)
        {
            Process _powercfg = new Process()
            {
                StartInfo = {
                        FileName = "powercfg",
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    },
            };

            string _serchScheme = default, 
                unlockFrequency = @"-attributes SUB_PROCESSOR 75b0ae3f-bce0-45a7-8c89-c9611c25e100 -ATTRIB_HIDE",
                inputPath = Settings.PathTempFiles + @"\UltimatePerformance.pow";

            try
            {
                if (isChoose)
                {
                    Parallel.Invoke(() =>
                    {
                        foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2\power", "SELECT InstanceID FROM Win32_PowerPlan WHERE IsActive=false").Get())
                        {
                            _serchScheme = Convert.ToString(managementObj["InstanceID"]);
                            _serchScheme = Regex.Match(_serchScheme, @"\{([^)]*)\}").Groups[1].Value;
                            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes\" + _serchScheme + "", "Description", string.Empty).ToString().Contains("-18") &&
                            Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes\" + _serchScheme + "", "FriendlyName", string.Empty).ToString().Contains("-19"))
                            {
                                using (_powercfg)
                                {
                                    _powercfg.StartInfo.Arguments = $"/setactive {_serchScheme}";
                                    _powercfg.Start();

                                    _powercfg.StartInfo.Arguments = unlockFrequency;
                                    _powercfg.Start();
                                }
                            }
                        }

                    });

                    if (string.IsNullOrEmpty(_serchScheme))
                    {
                        byte[] _byte = default;
                        using (MemoryStream fileOut = new MemoryStream(Properties.Resources.Ultimate_Performance_pow))
                        using (GZipStream gz = new GZipStream(fileOut, CompressionMode.Decompress))
                        using (MemoryStream ms = new MemoryStream())
                        {
                            gz.CopyTo(ms);
                            _byte = ms.ToArray();
                        }

                        Directory.CreateDirectory(Settings.PathTempFiles);
                        File.WriteAllBytes(Settings.PathTempFiles + @"\UltimatePerformance.pow", _byte);


                        string _guid = Guid.NewGuid().ToString("D");

                        using (_powercfg)
                        {
                            _powercfg.StartInfo.Arguments = $"-import \"{inputPath}\" {_guid}";
                            _powercfg.Start();

                            await Task.Delay(5);

                            _powercfg.StartInfo.Arguments = $"/setactive {_guid}";
                            _powercfg.Start();

                            _powercfg.StartInfo.Arguments = unlockFrequency;
                            _powercfg.Start();
                        }
                    }
                }
                else
                {
                    string _activePowerScheme = @"Microsoft:PowerPlan\\{" + Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes").GetValue("ActivePowerScheme").ToString() + "}";

                    Parallel.Invoke(() =>
                    {
                        foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2\power", "SELECT InstanceID FROM Win32_PowerPlan WHERE InstanceID !='" + _activePowerScheme + "'").Get())
                            _serchScheme = Convert.ToString(managementObj["InstanceID"]);
                    });

                    _serchScheme = Regex.Match(_serchScheme, @"\{([^)]*)\}").Groups[1].Value;

                    using (_powercfg)
                    {
                        _powercfg.StartInfo.Arguments = $"/setactive {_serchScheme}";
                        _powercfg.Start();
                    }
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message.ToString()); }

        }
    }
}
