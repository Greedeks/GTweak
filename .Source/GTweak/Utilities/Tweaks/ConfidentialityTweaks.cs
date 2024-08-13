using GTweak.Utilities.Helpers;
using GTweak.View;
using Microsoft.Win32;
using System.Diagnostics;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace GTweak.Utilities.Tweaks
{
    internal sealed class ConfidentialityTweaks : Firewall
    {
        private static readonly string[] schedulerTasks = {
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

        private static readonly string[] nvidiaTasks = {
                @"NvTmRepOnLogon_{B2FE1952-0186-46C3-BAEC-A80AA35AC5B8}",
                @"NvTmRep_{B2FE1952-0186-46C3-BAEC-A80AA35AC5B8}",
                @"NvTmMon_{B2FE1952-0186-46C3-BAEC-A80AA35AC5B8}" };

        private static readonly string[] telemetryTasks = {
                @"Microsoft\Office\Office ClickToRun Service Monitor",
                @"Microsoft\Office\OfficeTelemetry\AgentFallBack2016",
                @"Microsoft\Office\OfficeTelemetry\OfficeTelemetryAgentLogOn2016",
                @"Microsoft\Office\OfficeTelemetryAgentFallBack2016",
                @"Microsoft\Office\OfficeTelemetryAgentLogOn2016",
                @"Microsoft\Office\OfficeTelemetryAgentFallBack",
                @"Microsoft\Office\OfficeTelemetryAgentLogOn",
                @"Microsoft\Office\Office 15 Subscription Heartbeat" };

        private static readonly string[] hostsRules = {
                @"0.0.0.0 hk2sch130021829.wns.windows.com",
                @"0.0.0.0 v10-win.vortex.data.microsoft.com.akadns.net",
                @"0.0.0.0 watson.live.com",
                @"0.0.0.0 api.cortana.ai" };

        internal void ViewСonfidentiality(ConfidentialityView confidentialityV)
        {
            if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", null) == null ||
               Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", string.Empty).ToString() != "0" ||
               Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PolicyManager\current\device\Bluetooth", "AllowAdvertising", null) == null ||
               Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PolicyManager\current\device\Bluetooth", "AllowAdvertising", string.Empty).ToString() != "0")
                confidentialityV.TglButton1.StateNA = true;
            else
                confidentialityV.TglButton1.StateNA = false;


            if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Accessibility", "Enabled", null) == null ||
              Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Accessibility", "Enabled", string.Empty).ToString() != "0" ||
              Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SettingSync\Groups\BrowserSettings", "Enabled", null) == null ||
              Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SettingSync\Groups\BrowserSettings", "Enabled", string.Empty).ToString() != "0" ||
              Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Credentials", "Enabled", null) == null ||
              Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Credentials", "Enabled", string.Empty).ToString() != "0" ||
              Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Language", "Enabled", null) == null ||
              Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Language", "Enabled", string.Empty).ToString() != "0" ||
              Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Personalization", "Enabled", null) == null ||
              Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Personalization", "Enabled", string.Empty).ToString() != "0" ||
              Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Windows", "Enabled", null) == null ||
              Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Windows", "Enabled", string.Empty).ToString() != "0")
                confidentialityV.TglButton2.StateNA = true;
            else
                confidentialityV.TglButton2.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\WMI\Autologger\Diagtrack-Listener", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\WMI\Autologger\Diagtrack-Listener", "Start", string.Empty).ToString() != "0" ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Attachments", "SaveZoneInformation", null) == null ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Attachments", "SaveZoneInformation", string.Empty).ToString() != "1")
                confidentialityV.TglButton3.StateNA = true;
            else
                confidentialityV.TglButton3.StateNA = false;


            if (ViewTaskState(schedulerTasks) > 0)
                confidentialityV.TglButton4.StateNA = true;
            else
                confidentialityV.TglButton4.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\AppCompat", "DisableInventory", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\AppCompat", "DisableInventory", string.Empty).ToString() != "1")
                confidentialityV.TglButton5.StateNA = true;
            else
                confidentialityV.TglButton5.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\DataCollection", "AllowTelemetry", null) == null ||
              Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\DataCollection", "AllowTelemetry", string.Empty).ToString() != "0" ||
              Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\AppCompat", "AITEnable", null) == null ||
              Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\AppCompat", "AITEnable", string.Empty).ToString() != "0" ||
              Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowDeviceNameInTelemetry", null) == null ||
              Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowDeviceNameInTelemetry", string.Empty).ToString() != "0" ||
              Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackProgs", null) == null ||
              Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackProgs", string.Empty).ToString() != "0")
                confidentialityV.TglButton6.StateNA = true;
            else
                confidentialityV.TglButton6.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\TabletPC", "PreventHandwritingDataSharing", null) == null ||
               Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\TabletPC", "PreventHandwritingDataSharing", string.Empty).ToString() != "1" ||
               Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\HandwritingErrorReports", "PreventHandwritingErrorReports", null) == null ||
               Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\HandwritingErrorReports", "PreventHandwritingErrorReports", string.Empty).ToString() != "1" ||
               Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Input\TIPC", "Enabled", null) == null ||
               Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Input\TIPC", "Enabled", string.Empty).ToString() != "0")
                confidentialityV.TglButton7.StateNA = true;
            else
                confidentialityV.TglButton7.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\SQMClient\Windows", "CEIPEnable", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\SQMClient\Windows", "CEIPEnable", string.Empty).ToString() != "0")
                confidentialityV.TglButton8.StateNA = true;
            else
                confidentialityV.TglButton8.StateNA = false;


            if (ViewHosts(hostsRules) < 4)
                confidentialityV.TglButton9.StateNA = true;
            else
                confidentialityV.TglButton9.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors", "DisableLocation", null) == null ||
               Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors", "DisableLocation", string.Empty).ToString() != "1" ||
               Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors", "DisableLocationScripting", null) == null ||
               Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors", "DisableLocationScripting", string.Empty).ToString() != "1" ||
               Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors", "DisableWindowsLocationProvider", null) == null ||
               Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors", "DisableWindowsLocationProvider", string.Empty).ToString() != "1")
                confidentialityV.TglButton10.StateNA = true;
            else
                confidentialityV.TglButton10.StateNA = false;


            if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Siuf\Rules", "NumberOfSIUFInPeriod", null) == null ||
              Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Siuf\Rules", "NumberOfSIUFInPeriod", string.Empty).ToString() != "0" ||
              Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Siuf\Rules", "PeriodInNanoSeconds", null) == null ||
              Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Siuf\Rules", "PeriodInNanoSeconds", string.Empty).ToString() != "0" ||
              Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DataCollection", "DoNotShowFeedbackNotifications", null) == null ||
              Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DataCollection", "DoNotShowFeedbackNotifications", string.Empty).ToString() != "1")
                confidentialityV.TglButton11.StateNA = true;
            else
                confidentialityV.TglButton11.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Speech", "AllowSpeechModelUpdate", null) == null ||
             Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Speech", "AllowSpeechModelUpdate", string.Empty).ToString() != "0")
                confidentialityV.TglButton12.StateNA = true;
            else
                confidentialityV.TglButton12.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\CDPUserSvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\CDPUserSvc", "Start", string.Empty).ToString() != "4")
                confidentialityV.TglButton13.StateNA = true;
            else
                confidentialityV.TglButton13.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PolicyManager\current\device\System", "AllowExperimentation", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PolicyManager\current\device\System", "AllowExperimentation", string.Empty).ToString() != "0")
                confidentialityV.TglButton14.StateNA = true;
            else
                confidentialityV.TglButton14.StateNA = false;


            if (Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\DiagTrack") != null ||
                Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\dmwappushservice") != null)
                confidentialityV.TglButton15.StateNA = true;
            else
                confidentialityV.TglButton15.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\diagnosticshub.standardcollector.service", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\diagnosticshub.standardcollector.service", "Start", string.Empty).ToString() != "4")
                confidentialityV.TglButton16.StateNA = true;
            else
                confidentialityV.TglButton16.StateNA = false;


            if (SystemData.СomputerСonfiguration.СonfigurationData["GPU"].ToLower().Contains("nvidia"))
            {
                if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\NvTelemetryContainer", "Start", null) == null ||
                    Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\NvTelemetryContainer", "Start", string.Empty).ToString() != "4" ||
                    ViewTaskState(nvidiaTasks) > 0)
                    confidentialityV.TglButton17.StateNA = true;
                else
                    confidentialityV.TglButton17.StateNA = false;
            }
            else
                confidentialityV.TglButton17.StateNA = false;

            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\AppCompat", "DisableUAR", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\AppCompat", "DisableUAR", string.Empty).ToString() != "1" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Personalization", "NoLockScreenCamera", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Personalization", "NoLockScreenCamera", string.Empty).ToString() != "1")
                confidentialityV.TglButton18.StateNA = true;
            else
                confidentialityV.TglButton18.StateNA = false;

        }

        private byte ViewTaskState(string[] _tasklist)
        {
            byte _countTaskRun = 0;
            using (Microsoft.Win32.TaskScheduler.TaskService taskService = new Microsoft.Win32.TaskScheduler.TaskService())
            {
                foreach (string taskname in _tasklist)
                {
                    Microsoft.Win32.TaskScheduler.Task _task = taskService.GetTask(taskname);
                    if (_task != null)
                    {
                        if (_task.Enabled)
                            _countTaskRun++;
                    }
                }
            }
            return _countTaskRun;
        }

        internal byte ViewHosts(string[] _hostslist)
        {
            try
            {
                byte _count = 0;
                StreamReader streamReader = new StreamReader(Settings.PathSystemDisk + @"\Windows\System32\drivers\etc\hosts");
                string _hosts = streamReader.ReadToEnd();

                foreach (string hostsrules in _hostslist)
                {
                    if (_hosts.Contains(hostsrules))
                        _count++;
                }

                streamReader.Close();
                streamReader.Dispose();
                return _count;
            }
            catch { return 0; }
        }


        internal static void UseСonfidentiality(string tweak, bool isChoose)
        {
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
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\WMI\Autologger\Diagtrack-Listener", "Start", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Attachments", "SaveZoneInformation", 1, RegistryValueKind.DWord);
                        DisablingTasks(telemetryTasks);
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\WMI\Autologger\Diagtrack-Listener", "Start", 1, RegistryValueKind.DWord);
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Attachments", "SaveZoneInformation");
                        EnablingTasks(telemetryTasks);
                    }
                    break;
                case "TglButton4":
                    if (isChoose)
                    {
                        new Thread(() => DisablingTasks(schedulerTasks)).Start();
                        new Thread(() => DisablingTasks(schedulerTasks)).IsBackground = true;
                    }
                    else
                    {
                        new Thread(() => EnablingTasks(schedulerTasks)).Start();
                        new Thread(() => EnablingTasks(schedulerTasks)).IsBackground = true;
                    }
                    break;
                case "TglButton5":
                    if (isChoose)
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppCompat", "DisableInventory", 1, RegistryValueKind.DWord);
                    else
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppCompat", "DisableInventory");
                    break;
                case "TglButton6":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\DataCollection", "AllowTelemetry", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppCompat", "AITEnable", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowDeviceNameInTelemetry", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackProgs", 0, RegistryValueKind.DWord);
                    }
                    else
                    {
                        try
                        {
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\DataCollection", "AllowTelemetry", 1, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppCompat", "AITEnable", 1, RegistryValueKind.DWord);
                            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowDeviceNameInTelemetry");
                            RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackProgs", 1, RegistryValueKind.DWord);
                        }
                        catch (Exception ex) { Debug.WriteLine(ex.Message.ToString()); }
                    }
                    break;
                case "TglButton7":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\TabletPC", "PreventHandwritingDataSharing", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\HandwritingErrorReports", "PreventHandwritingErrorReports", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Input\TIPC", "Enabled", 0, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\TabletPC", "PreventHandwritingDataSharing");
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\HandwritingErrorReports", "PreventHandwritingErrorReports", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Input\TIPC", "Enabled", 1, RegistryValueKind.DWord);
                    }
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
                        BlockSpyDomain(isChoose);

                        try { File.Delete(Settings.PathSystemDisk + @"\Windows\System32\drivers\etc\hosts (Default GTweak)"); } 
                        catch (Exception ex) { Debug.WriteLine(ex.Message.ToString()); }

                        File.Move(Settings.PathSystemDisk + @"\Windows\System32\drivers\etc\hosts", Settings.PathSystemDisk + @"\Windows\System32\drivers\etc\hosts (Default GTweak)");

                        byte[] _byte = default;
                        using (MemoryStream fileOut = new MemoryStream(Properties.Resources.hosts))
                        using (GZipStream gz = new GZipStream(fileOut, CompressionMode.Decompress))
                        using (MemoryStream ms = new MemoryStream())
                        {
                            gz.CopyTo(ms);
                            _byte = ms.ToArray();
                        }
                        File.WriteAllBytes(Settings.PathSystemDisk + @"\Windows\System32\drivers\etc\hosts", _byte);
                    }
                    else
                    {
                        try
                        {
                            BlockSpyDomain(isChoose);
                            File.Copy(Settings.PathSystemDisk + @"\Windows\System32\drivers\etc\hosts (Default GTweak)", Settings.PathSystemDisk + @"\Windows\System32\drivers\etc\hosts", true);
                            File.Delete(Settings.PathSystemDisk + @"\Windows\System32\drivers\etc\hosts (Default GTweak)");
                        }
                        catch (Exception ex) { Debug.WriteLine(ex.Message.ToString()); }
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
                    if (isChoose)
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\CDPUserSvc", "Start", 4, RegistryValueKind.DWord);
                    else
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\CDPUserSvc", "Start", 2, RegistryValueKind.DWord);
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
                    if (isChoose)
                    {
                        TrustedInstaller.CreateProcessAsTrustedInstaller(Settings.PID, "cmd.exe /c reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\diagnosticshub.standardcollector.service /t REG_DWORD /v Start /d 4 /f");
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\diagnosticshub.standardcollector.service", "Start", 4, RegistryValueKind.DWord);
                    }
                    else
                    {
                        TrustedInstaller.CreateProcessAsTrustedInstaller(Settings.PID, "cmd.exe /c reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\diagnosticshub.standardcollector.service /t REG_DWORD /v Start /d 3 /f");
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\diagnosticshub.standardcollector.service", "Start", 3, RegistryValueKind.DWord);
                    }
                    break;
                case "TglButton17":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\NvTelemetryContainer", "Start", 4, RegistryValueKind.DWord);
                        DisablingTasks(nvidiaTasks);
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\NvTelemetryContainer", "Start", 2, RegistryValueKind.DWord);
                        EnablingTasks(nvidiaTasks);
                    }
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


        private static void EnablingTasks(string[] _tasklist)
        {
            Parallel.Invoke(() =>
            {
                Microsoft.Win32.TaskScheduler.TaskService taskService = new Microsoft.Win32.TaskScheduler.TaskService();
                foreach (string taskname in _tasklist)
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

        private static void DisablingTasks(string[] _tasklist)
        {
            Parallel.Invoke(() =>
            {
                Microsoft.Win32.TaskScheduler.TaskService taskService = new Microsoft.Win32.TaskScheduler.TaskService();
                foreach (string taskname in _tasklist)
                {
                    Microsoft.Win32.TaskScheduler.Task _task = taskService.GetTask(taskname);
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

    }
}
