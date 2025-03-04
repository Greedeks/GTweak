﻿using GTweak.Utilities.Control;
using GTweak.Utilities.Helpers;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace GTweak.Utilities.Tweaks
{
    internal class WindowsDefender
    {
        internal void Enable()
        {
            TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, "cmd.exe /c reg delete HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\AppHost /v EnableWebContentEvaluation /f & " +
                "reg delete HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer /v SmartScreenEnabled /f & " +
                "reg delete HKLM\\SOFTWARE\\Policies\\Microsoft\\MicrosoftEdge\\PhishingFilter /v EnabledV9 /f & " +
                "reg delete HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\System /v EnableSmartScreen /f & " +
                "reg delete \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\" /v DisableAntiSpyware /f & " +
                "reg delete \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\" /v DisableAntiVirus /f & " +
                "reg delete \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\SmartScreen\" /v ConfigureAppInstallControl /f & " +
                "reg delete \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\SmartScreen\" /v ConfigureAppInstallControlEnabled /f & " +
                "reg delete \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\\Real-Time Protection\" /v DisableBehaviorMonitoring /f & " +
                "reg delete \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\\Real-Time Protection\" /v DisableOnAccessProtection /f & " +
                "reg delete \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\\Real-Time Protection\" /v DisableScanOnRealtimeEnable /f & " +
                "reg delete \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\\Real-Time Protection\" /v DisableIOAVProtection /f & " +
                "reg delete \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\\Real-Time Protection\" /v DisableRealtimeMonitoring /f & " +
                "reg delete \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\\Real-Time Protection\" /v LocalSettingOverrideDisableOnAccessProtection /f & " +
                "reg delete \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\\Real-Time Protection\" /v LocalSettingOverrideRealtimeScanDirection /f & " +
                "reg delete \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\\Real-Time Protection\" /v LocalSettingOverrideDisableIOAVProtection /f & " +
                "reg delete \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\\Real-Time Protection\" /v LocalSettingOverrideDisableBehaviorMonitoring /f & " +
                "reg delete \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\\Real-Time Protection\" /v LocalSettingOverrideDisableIntrusionPreventionSystem /f & " +
                "reg delete \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\\Real-Time Protection\" /v LocalSettingOverrideDisableRealtimeMonitoring /f & " +
                "reg delete HKLM\\SOFTWARE\\Policies\\Microsoft\\MRT /v DontOfferThroughWUA /f & " +
                "reg delete HKLM\\SOFTWARE\\Policies\\Microsoft\\MRT /v DontReportInfectionInformation /f & " +
                "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\" / t REG_DWORD /v PUAProtection /d 2 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Spynet\" /t REG_DWORD /v SpyNetReporting /d 1 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Spynet\" /t REG_DWORD /v SubmitSamplesConsent /d 0 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\\Features\" /t REG_DWORD /v TamperProtection /d 1 /f & " +
                "reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\SecurityHealthService /t REG_DWORD /v Start /d 2 /f &" +
                "reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\WinDefend /t REG_DWORD /v Start  /d 2 /f & " +
                "reg delete HKLM\\SYSTEM\\CurrentControlSet\\Services\\WinDefend /t REG_DWORD /v AutorunsDisabled /f & " +
                "reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\WdNisSvc /t REG_DWORD /v Start  /d 3 /f");

            TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, $"cmd.exe /c \"rename \"" + Path.Combine(StoragePaths.SystemDisk, @"Windows\System32\BlockSS.exe") + "\" smartscreen.exe\"");
            TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, $"cmd.exe /c \"rename \"" + Path.Combine(StoragePaths.SystemDisk, @"Program Files\Windows Defender\BlockAntimalware.exe") + "\" MsMpEng.exe\"");
            TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, $"cmd.exe /c \"rename \"" + Path.Combine(StoragePaths.SystemDisk, @"Program Files\Windows Defender\BlockAntimalwareCore.exe") + "\" MpDefenderCoreService.exe\"");
            TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, $"cmd.exe /c for /d %D in (\"{Path.Combine(StoragePaths.SystemDisk, @"ProgramData\Microsoft\Windows Defender\Platform\*")}\") do if exist \"%D\\BlockAntimalware.exe\" ren \"%D\\BlockAntimalware.exe\" MsMpEng.exe");
            TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, $"cmd.exe /c for /d %D in (\"{Path.Combine(StoragePaths.SystemDisk, @"ProgramData\Microsoft\Windows Defender\Platform\*")}\") do if exist \"%D\\BlockAntimalwareCore.exe\" ren \"%D\\BlockAntimalwareCore.exe\" MpDefenderCoreService.exe");

            RunCmdCommand("sc start WinDefend");
            RunPowerShellCommand("Start-Service -Name WinDefend; Set-Service -Name WinDefend -StartupType Automatic");

            RegistryHelp.CreateFolder(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer");
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer", "SmartScreenEnabled", "On", RegistryValueKind.String);
            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "EnableSmartScreen");
            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender");
            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\Policy Manager");
            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\MpEngine");
            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\SmartScreen");
            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\Real-Time Protection");
            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\Reportin");
            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\SpyNet");
            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\UX Configuration");
            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender Security Center");
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows Defender", "PUAProtection", 2, RegistryValueKind.DWord);

            RunPowerShellCommand("Set-MpPreference -DisableIOAVProtection $false");
            RunPowerShellCommand("Set-MpPreference -DisableRealtimeMonitoring $false");
            RunPowerShellCommand("Set-MpPreference -DisableBehaviorMonitoring $false");
            RunPowerShellCommand("Set-MpPreference -DisableBlockAtFirstSeen $false");
            RunPowerShellCommand("Set-MpPreference -DisableIOAVProtection $false");
            RunPowerShellCommand("Set-MpPreference -DisablePrivacyMode $false");
            RunPowerShellCommand("Set-MpPreference -SignatureDisableUpdateOnStartupWithoutEngine $false");
            RunPowerShellCommand("Set-MpPreference -DisableArchiveScanning $false");
            RunPowerShellCommand("Set-MpPreference -DisableIntrusionPreventionSystem $false");
            RunPowerShellCommand("Set-MpPreference -DisableScriptScanning $false");
            RunPowerShellCommand("Set-MpPreference -SubmitSamplesConsent 1");
            RunPowerShellCommand("Set-MpPreference -MAPSReporting 2");
            RunPowerShellCommand("Set-MpPreference -PUAProtection 1");
            RunPowerShellCommand("Set-MpPreference -PUAProtection Enabled");
            RunPowerShellCommand(@"Set-ExecutionPolicy UnRestricted; Set-ItemProperty -Path HKLM:\SOFTWARE\Microsoft\Windows Defender\Features -Name TamperProtection -Value 0x00000001");

            RegistryHelp.CreateFolder(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\DomainProfile");
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\DomainProfile", "DisableNotifications", 0, RegistryValueKind.DWord);
            RegistryHelp.CreateFolder(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\PrivateProfile");
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\PrivateProfile", "DisableNotifications", 0, RegistryValueKind.DWord);
            RegistryHelp.CreateFolder(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\StandardProfile");
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\StandardProfile", "DisableNotifications", 0, RegistryValueKind.DWord);
            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender Security Center");
            RegistryHelp.DeleteFolderTree(Registry.CurrentUser, @"SOFTWARE\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppContainer\Storage\microsoft.microsoftedge_8wekyb3d8bbwe\MicrosoftEdge");

            EnableTask("Microsoft\\Windows\\ExploitGuard\\ExploitGuard MDM policy Refresh");
            EnableTask("Microsoft\\Windows\\Windows Defender\\Windows Defender Cache Maintenance");
            EnableTask("Microsoft\\Windows\\Windows Defender\\Windows Defender Cleanup");
            EnableTask("Microsoft\\Windows\\Windows Defender\\Windows Defender Scheduled Scan");
            EnableTask("Microsoft\\Windows\\Windows Defender\\Windows Defender Verification");

            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\MicrosoftEdge\PhishingFilter");
            RegistryHelp.CreateFolder(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\CI\Config");
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\CI\Config", "VulnerableDriverBlocklistEnable", 1, RegistryValueKind.DWord);
            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WTDS");
            RegistryHelp.CreateFolder(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\DeviceGuard\Scenarios\HypervisorEnforcedCodeIntegrity");
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\DeviceGuard\Scenarios\HypervisorEnforcedCodeIntegrity", "Enabled", 1, RegistryValueKind.DWord);
            RegistryHelp.DeleteValue(Registry.LocalMachine, @"Software\Microsoft\Windows\CurrentVersion\AppHost", "EnableWebContentEvaluation");
            RegistryHelp.DeleteValue(Registry.LocalMachine, @"Software\Microsoft\Windows\CurrentVersion\AppHost", "PreventOverride");
            RegistryHelp.CreateFolder(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "SecurityHealth", @"%windir%\system32\SecurityHealthSystray.exe", RegistryValueKind.ExpandString);

            RunPowerShellCommand(@"Set-ExecutionPolicy UnRestricted; Set-ItemProperty -Path HKLM:\SYSTEM\CurrentControlSet\Services\WinDefend -Name Start -Value 0x00000002");
            RunPowerShellCommand(@"Set-ExecutionPolicy UnRestricted; Set-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows Defender\Real-Time Protection' -Name DisableBehaviorMonitoring -Value 0x00000000");
            RunPowerShellCommand(@"Set-ExecutionPolicy UnRestricted; Set-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows Defender\Real-Time Protection' -Name DisableIOAVProtection -Value 0x00000000");
            RunPowerShellCommand(@"Set-ExecutionPolicy UnRestricted; Set-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows Defender\Real-Time Protection' -Name DisableOnAccessProtection -Value 0x00000000");
            RunPowerShellCommand(@"Set-ExecutionPolicy UnRestricted; Set-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows Defender\Real-Time Protection' -Name DisableRealtimeMonitoring -Value 0x00000000");
            RunPowerShellCommand(@"Set-ExecutionPolicy UnRestricted; Set-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows Defender\Real-Time Protection' -Name DisableScanOnRealtimeEnable -Value 0x00000000");
            RunPowerShellCommand(@"Set-ExecutionPolicy UnRestricted; Set-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows Defender\Real-Time Protection' -Name DisableScriptScanning -Value 0x00000000");
            RunPowerShellCommand(@"Set-ExecutionPolicy UnRestricted; Set-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows Defender\Real-Time Protection' -Name DisableScriptScanning -Value 0x00000000");

            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run", "SecurityHealth", new byte[] { 0002, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 }, RegistryValueKind.Binary);
            RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "Windows Defender", "\"%ProgramFiles%\\Windows Defender\\MSASCui.exe\"-runkey", RegistryValueKind.String);
            RunProcess(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + @"\Windows Defender\MpCmdRun.exe", "-SignatureUpdate");

            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WinDefend", "Start", "2", RegistryValueKind.DWord, true);
            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WinDefend", "AutorunsDisabled");
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SecurityHealthService", "Start", "2", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows Defender\Features", "TamperProtection", "1", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WdNisSvc", "Start", "3", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\Sense", "Start", "3", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WdFilter", "Start", "0", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WdBoot", "Start", "0", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WdNisDrv", "Start", "3", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\MDCoreSvc", "Start", "3", RegistryValueKind.DWord);

            CloseDefenderSettings();

            RunPowerShellCommand(@"Get-AppXpackage Microsoft.WindowsDefender | Foreach {Add-AppxPackage -DisableDevelopmentMode -Register ""$($_.InstallLocation)\AppXManifest.xml""}");

            RegistryHelp.Write(Registry.ClassesRoot, @"*\shellex\ContextMenuHandlers\EPP", "", "{09A47860-11B0-4DA5-AFA5-26D86198A780}", RegistryValueKind.String);
            RegistryHelp.Write(Registry.ClassesRoot, @"Directory\shellex\ContextMenuHandlers\EPP", "", "{09A47860-11B0-4DA5-AFA5-26D86198A780}", RegistryValueKind.String);
            RegistryHelp.Write(Registry.ClassesRoot, @"Drive\shellex\ContextMenuHandlers\EPP", "", "{09A47860-11B0-4DA5-AFA5-26D86198A780}", RegistryValueKind.String);
            RegistryHelp.Write(Registry.ClassesRoot, @"CLSID\{09A47860-11B0-4DA5-AFA5-26D86198A780}\InprocServer32", "", StoragePaths.SystemDisk + @"Program Files\Windows Defender\shellext.dll", RegistryValueKind.String);
            RegistryHelp.Write(Registry.ClassesRoot, @"CLSID\{09A47860-11B0-4DA5-AFA5-26D86198A780}\InprocServer32", "ThreadingModel", "Apartment", RegistryValueKind.String);

            RunProcess(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + @"\Windows Defender\MpCmdRun.exe", "-SignatureUpdate");
        }

        internal async void Disable()
        {
            KillProcess("smartscreen");
            KillProcess("MsMpEng");

            RegistryHelp.CreateFolder(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer");
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer", "SmartScreenEnabled", "Off", RegistryValueKind.String);
            RegistryHelp.CreateFolder(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System");
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "EnableSmartScreen", 0, RegistryValueKind.DWord);

            RunPowerShellCommand("Set-MpPreference -DisableIOAVProtection $true");
            RunPowerShellCommand("Set-MpPreference -DisableRealtimeMonitoring $true");
            RunPowerShellCommand("Set-MpPreference -DisableBehaviorMonitoring $true");
            RunPowerShellCommand("Set-MpPreference -DisableBlockAtFirstSeen $true");
            RunPowerShellCommand("Set-MpPreference -DisableIOAVProtection $true");
            RunPowerShellCommand("Set-MpPreference -DisablePrivacyMode $true");
            RunPowerShellCommand("Set-MpPreference -SignatureDisableUpdateOnStartupWithoutEngine $true");
            RunPowerShellCommand("Set-MpPreference -DisableArchiveScanning $true");
            RunPowerShellCommand("Set-MpPreference -DisableIntrusionPreventionSystem $true");
            RunPowerShellCommand("Set-MpPreference -DisableScriptScanning $true");
            RunPowerShellCommand("Set-MpPreference -SubmitSamplesConsent 2");
            RunPowerShellCommand("Set-MpPreference -MAPSReporting 0");
            RunPowerShellCommand("Set-MpPreference -HighThreatDefaultAction 6 -Force");
            RunPowerShellCommand("Set-MpPreference -ModerateThreatDefaultAction 6");
            RunPowerShellCommand("Set-MpPreference -LowThreatDefaultAction 6");
            RunPowerShellCommand("Set-MpPreference -SevereThreatDefaultAction 6");

            RunPowerShellCommand("Set-MpPreference -PUAProtection 0");
            RunPowerShellCommand("Set-MpPreference -PUAProtection Disabled");

            RegistryHelp.CreateFolder(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender");
            RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender", "DisableAntiSpyware", 1, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender", "DisableAntiVirus", 1, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender", "ServiceKeepAlive", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender", "AllowFastService", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender", "DisableRoutinelyTakingAction", 1, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender", "HideExclusionsFromLocalAdmins", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender", "ServiceKeepAlive", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender", "AllowFastServiceStartup", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender", "PUAProtection", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender", "DisableLocalAdminMerge", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows Defender", "PUAProtection", 0, RegistryValueKind.DWord);

            RegistryHelp.CreateFolder(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\DomainProfile");
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\DomainProfile", "DisableNotifications", 1, RegistryValueKind.DWord);
            RegistryHelp.CreateFolder(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\PrivateProfile");
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\PrivateProfile", "DisableNotifications", 1, RegistryValueKind.DWord);
            RegistryHelp.CreateFolder(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\StandardProfile");
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\StandardProfile", "DisableNotifications", 1, RegistryValueKind.DWord);
            RegistryHelp.CreateFolder(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender Security Center");
            RegistryHelp.CreateFolder(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender Security Center\Notifications");
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender Security Center\Notifications", "DisableEnhancedNotifications", 1, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender Security Center\Notifications", "DisableNotifications", 1, RegistryValueKind.DWord);
            RegistryHelp.CreateFolder(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender Security Center\App and Browser protection");
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender Security Center\App and Browser protection", "DisallowExploitProtectionOverride", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender Security Center\App and Browser protection", "UILockdown", 1, RegistryValueKind.DWord);
            RegistryHelp.CreateFolder(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\Features");
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender\Features", "DeviceControlEnabled", 0, RegistryValueKind.DWord);
            RegistryHelp.CreateFolder(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\MpEngine");
            RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\MpEngine", "MpEnablePus", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\MpEngine", "DisableGradualRelease", 1, RegistryValueKind.DWord);

            RegistryHelp.CreateFolder(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\Real-Time Protection");
            RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\Real-Time Protection", "LocalSettingOverrideDisableOnAccessProtection", 0, RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\Real-Time Protection", "LocalSettingOverrideRealtimeScanDirection", 0, RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\Real-Time Protection", "LocalSettingOverrideDisableIOAVProtection", 0, RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\Real-Time Protection", "LocalSettingOverrideDisableBehaviorMonitoring", 0, RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\Real-Time Protection", "LocalSettingOverrideDisableIntrusionPreventionSystem", 0, RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\Real-Time Protection", "LocalSettingOverrideDisableRealtimeMonitoring", 0, RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\Real-Time Protection", "DisableBehaviorMonitoring", 1, RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\Real-Time Protection", "DisableIOAVProtection", 1, RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\Real-Time Protection", "DisableOnAccessProtection", 1, RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\Real-Time Protection", "DisableRealtimeMonitoring", 1, RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\Real-Time Protection", "DisableRoutinelyTakingAction", 1, RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\Real-Time Protection", "DisableScanOnRealtimeEnable", 1, RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\Real-Time Protection", "DisableScriptScanning", 1, RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\Real-Time Protection", "DisableRawWriteNotification", 1, RegistryValueKind.DWord, true);

            RegistryHelp.CreateFolder(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\UX Configuration");
            RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\UX Configuration", "Notification_Suppress", 1, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\UX Configuration", "SuppressRebootNotification", 1, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\UX Configuration", "UILockdown", 1, RegistryValueKind.DWord);
            RegistryHelp.CreateFolder(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender Security Center\Device performance and health");
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender Security Center\Device performance and health", "UILockdown", 1, RegistryValueKind.DWord);
            RegistryHelp.CreateFolder(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender Security Center\Family optionsObj");
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender Security Center\Family optionsObj", "UILockdown", 1, RegistryValueKind.DWord);
            RegistryHelp.CreateFolder(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\Reportin");
            RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\Reportin", "DisableEnhancedNotifications", 1, RegistryValueKind.DWord);

            RunProcess(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + @"\Windows Defender\MpCmdRun.exe", "-RemoveDefinitions -All");

            RunPowerShellCommand(@"Set-ExecutionPolicy UnRestricted; Set-ItemProperty -Path HKLM:\SOFTWARE\Microsoft\Windows Defender\Features -Name TamperProtection -Value 0x00000000");

            RegistryHelp.CreateFolder(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\SpyNet");
            RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\SpyNet", "DisableBlockAtFirstSeen", 1, RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\SpyNet", "SpynetReporting", 0, RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\SpyNet", "SubmitSamplesConsent", 2, RegistryValueKind.DWord, true);
            RegistryHelp.CreateFolder(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\CI\Config");
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\CI\Config", "EnabledV9", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\CI\Config", "VulnerableDriverBlocklistEnable", 0, RegistryValueKind.DWord);
            RegistryHelp.CreateFolder(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\DeviceGuard\Scenarios\HypervisorEnforcedCodeIntegrity");
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\DeviceGuard\Scenarios\HypervisorEnforcedCodeIntegrity", "Enabled", 0, RegistryValueKind.DWord);
            RegistryHelp.CreateFolder(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WTDS\Components");
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WTDS\Components", "ServiceEnabled", "0", RegistryValueKind.DWord);
            RegistryHelp.CreateFolder(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\MicrosoftEdge\PhishingFilter");
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\MicrosoftEdge\PhishingFilter", "EnabledV9", "0", RegistryValueKind.DWord);
            RegistryHelp.CreateFolder(Registry.LocalMachine, @"Software\Microsoft\Windows\CurrentVersion\AppHost");
            RegistryHelp.Write(Registry.LocalMachine, @"Software\Microsoft\Windows\CurrentVersion\AppHost", "EnableWebContentEvaluation", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"Software\Microsoft\Windows\CurrentVersion\AppHost", "PreventOverride", 0, RegistryValueKind.DWord);
            RegistryHelp.CreateFolder(Registry.CurrentUser, @"SOFTWARE\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppContainer\Storage\microsoft.microsoftedge_8wekyb3d8bbwe\MicrosoftEdge");
            RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppContainer\Storage\microsoft.microsoftedge_8wekyb3d8bbwe\MicrosoftEdge", "EnabledV9", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppContainer\Storage\microsoft.microsoftedge_8wekyb3d8bbwe\MicrosoftEdge", "PreventOverride", 0, RegistryValueKind.DWord);

            DisableTask("Microsoft\\Windows\\ExploitGuard\\ExploitGuard MDM policy Refresh");
            DisableTask("Microsoft\\Windows\\Windows Defender\\Windows Defender Cache Maintenance");
            DisableTask("Microsoft\\Windows\\Windows Defender\\Windows Defender Cleanup");
            DisableTask("Microsoft\\Windows\\Windows Defender\\Windows Defender Scheduled Scan");
            DisableTask("Microsoft\\Windows\\Windows Defender\\Windows Defender Verification");

            RunPowerShellCommand(@"Set-ExecutionPolicy UnRestricted; Set-ItemProperty -Path HKLM:\SYSTEM\CurrentControlSet\Services\WinDefend -Name Start -Value 0x00000004");
            RunPowerShellCommand(@"Set-ExecutionPolicy UnRestricted; Set-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows Defender\Real-Time Protection' -Name DisableBehaviorMonitoring -Value 0x00000001");
            RunPowerShellCommand(@"Set-ExecutionPolicy UnRestricted; Set-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows Defender\Real-Time Protection' -Name DisableIOAVProtection -Value 0x00000001");
            RunPowerShellCommand(@"Set-ExecutionPolicy UnRestricted; Set-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows Defender\Real-Time Protection' -Name DisableOnAccessProtection -Value 0x00000001");
            RunPowerShellCommand(@"Set-ExecutionPolicy UnRestricted; Set-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows Defender\Real-Time Protection' -Name DisableRealtimeMonitoring -Value 0x00000001");
            RunPowerShellCommand(@"Set-ExecutionPolicy UnRestricted; Set-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows Defender\Real-Time Protection' -Name DisableScanOnRealtimeEnable -Value 0x00000001");
            RunPowerShellCommand(@"Set-ExecutionPolicy UnRestricted; Set-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows Defender\Real-Time Protection' -Name DisableScriptScanning -Value 0x00000001");
            RunPowerShellCommand(@"Set-ExecutionPolicy UnRestricted; Set-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows Defender\Real-Time Protection' -Name DisableScriptScanning -Value 0x00000001");


            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run", "SecurityHealth", new byte[] { 0099, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99 }, RegistryValueKind.Binary);
            RegistryHelp.DeleteValue(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "Windows Defender");
            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "SecurityHealth");

            CloseDefenderSettings();

            string defenderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"Microsoft\Windows Defender");

            if (Directory.Exists(defenderPath))
            {
                string defenderScansPath = Path.Combine(defenderPath, @"Scans");

                if (Directory.Exists(defenderScansPath)) DeleteDir(defenderScansPath);
            }

            await Task.Delay(1000);

            TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, "cmd.exe /c taskkill /f /im MsMpEng.exe & reg add HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\AppHost /t REG_DWORD /v EnableWebContentEvaluation /d 0 /f & " +
                "reg add HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer /t REG_SZ /v SmartScreenEnabled /d \"off\" /f & " +
                "reg add HKLM\\SOFTWARE\\Policies\\Microsoft\\MicrosoftEdge\\PhishingFilter /t REG_DWORD /v EnabledV9 /d 0 /f & " +
                "reg add HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\System /t REG_DWORD /v EnableSmartScreen /d 0 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\" /t REG_DWORD /v DisableAntiSpyware /d 1 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\" /t REG_DWORD /v DisableAntiVirus /d 1 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\\Real-Time Protection\" /t REG_DWORD /v LocalSettingOverrideDisableOnAccessProtection /d 0 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\\Real-Time Protection\" /t REG_DWORD /v LocalSettingOverrideRealtimeScanDirection /d 0 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\\Real-Time Protection\" /t REG_DWORD /v LocalSettingOverrideDisableIOAVProtection /d 0 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\\Real-Time Protection\" /t REG_DWORD /v LocalSettingOverrideDisableBehaviorMonitoring /d 0 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\\Real-Time Protection\" /t REG_DWORD /v LocalSettingOverrideDisableIntrusionPreventionSystem /d 0 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\\Real-Time Protection\" /t REG_DWORD /v LocalSettingOverrideDisableRealtimeMonitoring /d 0 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\\Real-Time Protection\" /t REG_DWORD /v DisableBehaviorMonitoring /d 1 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\\Real-Time Protection\" /t REG_DWORD /v DisableOnAccessProtection /d 1 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\\Real-Time Protection\" /t REG_DWORD /v DisableScanOnRealtimeEnable /d 1 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\\Real-Time Protection\" /t REG_DWORD /v DisableIOAVProtection /d 1 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\\Real-Time Protection\" /t REG_DWORD /v DisableRealtimeMonitoring /d 1 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Spynet\" /t REG_DWORD /v SpyNetReporting /d 0 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Spynet\" /t REG_DWORD /v SubmitSamplesConsent /d 2 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\\Features\" /t REG_DWORD /v TamperProtection /d 0 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\" / t REG_DWORD /v PUAProtection /d 0 /f & " +
                "reg add HKLM\\SOFTWARE\\Policies\\Microsoft\\MRT /t REG_DWORD /v DontOfferThroughWUA /d 1 /f & " +
                "reg add HKLM\\SOFTWARE\\Policies\\Microsoft\\MRT /t REG_DWORD /v DontReportInfectionInformation /d 1 /f & " +
                "reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\SecurityHealthService /t REG_DWORD /v Start /d 4 /f &" +
                "reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\SmartScreen\" /t REG_SZ /v ConfigureAppInstallControl /d \"Anywhere\" /f & " +
                "reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\SmartScreen\" /t REG_DWORD /v ConfigureAppInstallControlEnabled /d 0 /f & " +
                "reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\WinDefend /t REG_DWORD /v Start  /d 4 /f & " +
                "reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\WinDefend /t REG_DWORD /v AutorunsDisabled / d 3 /f & " +
                "reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\WdNisSvc /t REG_DWORD /v Start  /d 4 /f");

            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WinDefend", "Start", "4", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WinDefend", "AutorunsDisabled", "3", RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SecurityHealthService", "Start", "4", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows Defender\Features", "TamperProtection", "0", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WdNisSvc", "Start", "4", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\Sense", "Start", "4", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WdFilter", "Start", "4", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WdBoot", "Start", "4", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WdNisDrv", "Start", "4", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\MDCoreSvc", "Start", "4", RegistryValueKind.DWord);

            RunCmdCommand("sc config WinDefend start= disabled");
            RunPowerShellCommand("Stop-Service -Name WinDefend; Set-Service -Name WinDefend -StartupType Disabled");

            RegistryHelp.DeleteFolderTree(Registry.ClassesRoot, @"*\shellex\ContextMenuHandlers\EPP");
            RegistryHelp.DeleteFolderTree(Registry.ClassesRoot, @"Directory\shellex\ContextMenuHandlers\EPP");
            RegistryHelp.DeleteFolderTree(Registry.ClassesRoot, @"Drive\shellex\ContextMenuHandlers\EPP");
            RegistryHelp.DeleteFolderTree(Registry.ClassesRoot, @"CLSID\{09A47860-11B0-4DA5-AFA5-26D86198A780}\InprocServer32");

            KillProcess("smartscreen");
            TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, $"cmd.exe /c \"rename \"" + Path.Combine(StoragePaths.SystemDisk, @"Windows\System32\smartscreen.exe") + "\" BlockSS.exe\"");
            KillProcess("MsMpEng");
            TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, $"cmd.exe /c \"rename \"" + Path.Combine(StoragePaths.SystemDisk, @"Program Files\Windows Defender\MsMpEng.exe") + "\" BlockAntimalware.exe\"");
            TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, $"cmd.exe /c for /d %D in (\"{Path.Combine(StoragePaths.SystemDisk, @"ProgramData\Microsoft\Windows Defender\Platform\*")}\") do if exist \"%D\\MsMpEng.exe\" ren \"%D\\MsMpEng.exe\" BlockAntimalware.exe");
            KillProcess("MpDefenderCoreService");
            TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, $"cmd.exe /c \"rename \"" + Path.Combine(StoragePaths.SystemDisk, @"Program Files\Windows Defender\MpDefenderCoreService.exe") + "\" BlockAntimalwareCore.exe\"");
            TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, $"cmd.exe /c for /d %D in (\"{Path.Combine(StoragePaths.SystemDisk, @"ProgramData\Microsoft\Windows Defender\Platform\*")}\") do if exist \"%D\\MpDefenderCoreService.exe\" ren \"%D\\MpDefenderCoreService.exe\" BlockAntimalwareCore.exe");
        }

        private void KillProcess(string getName)
        {
            foreach (Process process in Process.GetProcessesByName(getName))
            {
                try { process.Kill(); }
                catch (Exception ex) { Debug.WriteLine(ex.Message); }
            }
        }

        private void RunPowerShellCommand(string command)
        {
            RunProcess(Path.Combine(Environment.SystemDirectory, @"WindowsPowerShell\v1.0\powershell.exe"),
                "-NoLogo -NonInteractive -NoProfile -ExecutionPolicy Bypass -EncodedCommand \"" +
                Convert.ToBase64String(Encoding.Unicode.GetBytes(command)) + "\"");
        }

        private void RunCmdCommand(string command) => RunProcess(Path.Combine(Environment.SystemDirectory, "Cmd.exe"), "/d /q /c " + command);

        private void RunProcess(string path, string arguments)
        {
            try
            {
                Process comandoAEjecutar = new Process();
                comandoAEjecutar.StartInfo.FileName = path;
                comandoAEjecutar.StartInfo.Arguments = arguments;
                comandoAEjecutar.StartInfo.UseShellExecute = false;
                comandoAEjecutar.StartInfo.RedirectStandardOutput = true;
                comandoAEjecutar.StartInfo.RedirectStandardError = true;
                comandoAEjecutar.StartInfo.CreateNoWindow = true;
                comandoAEjecutar.Start();

                comandoAEjecutar.StandardOutput.ReadToEnd();
                return;
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
        }

        private void EnableTask(string path) => SetTask(path, "/Enable");

        private void DisableTask(string path) => SetTask(path, "/Disable");

        private void SetTask(string path, string param) => RunProcess(Path.Combine(Environment.SystemDirectory, @"schtasks.exe"), "/Change /TN \"" + path + "\" " + param);

        private void CloseDefenderSettings() => KillProcess("SecHealthUI");

        private void DeleteDir(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);

            foreach (FileInfo file in dir.GetFiles())
            {
                try { file.Delete(); }
                catch (Exception ex) { Debug.WriteLine(ex.Message); }
            }

            foreach (DirectoryInfo subDir in dir.GetDirectories())
            {
                try { subDir.Delete(true); }
                catch { DeleteDir(subDir.FullName); }
            }
        }
    }
}

