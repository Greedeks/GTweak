using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace GTweak.Utilities.Tweaks.DefenderManager
{
    internal class WindowsDefender : BackupRights
    {
        internal static void Activate()
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
                "reg delete HKLM\\SYSTEM\\CurrentControlSet\\Services\\WinDefend /t REG_DWORD /v AutorunsDisabled /f & " +
                "reg delete \"HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Image File Execution Options\\MpDefenderCoreService.exe\" /f & " +
                "reg delete \"HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Image File Execution Options\\SgrmBroker.exe\" /f & " +
                "reg delete \"HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Image File Execution Options\\smartscreen.exe\" /f & " +
                "reg delete \"HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Image File Execution Options\\MsMpEng.exe\" /v Debugger /f & " +
                "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Image File Execution Options\\MsMpEng.exe\" /t REG_DWORD /v CFGOptions /d 1 /f");

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
            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Microsoft Antimalware");
            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender\Signature Updates");
            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\Security Center", "FirstRunDisabled");
            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\Security Center", "AntiVirusOverride");
            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\Security Center", "FirewallOverride");
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

            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\DomainProfile", "DisableNotifications", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\PrivateProfile", "DisableNotifications", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\StandardProfile", "DisableNotifications", 0, RegistryValueKind.DWord);
            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender Security Center");
            RegistryHelp.DeleteFolderTree(Registry.CurrentUser, @"SOFTWARE\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppContainer\Storage\microsoft.microsoftedge_8wekyb3d8bbwe\MicrosoftEdge");

            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\WMI\Autologger\DefenderApiLogger", "Start", 1, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\WMI\Autologger\DefenderAuditLogger", "Start", 1, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\WINEVT\Channels\Microsoft-Windows-Windows Defender/Operational", "Enabled", 1, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\WINEVT\Channels\Microsoft-Windows-Security-Diagnostic/Operational", "Enabled", 1, RegistryValueKind.DWord);

            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows Defender\Cloud", "DisableBlockAtFirstSeen");
            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows Defender\Cloud", "CaptureBetaFeatures");

            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\MicrosoftEdge\PhishingFilter");
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\CI\Config", "VulnerableDriverBlocklistEnable", 1, RegistryValueKind.DWord);
            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WTDS");
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\DeviceGuard\Scenarios\HypervisorEnforcedCodeIntegrity", "Enabled", 1, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\DeviceGuard", "EnableVirtualizationBasedSecurity", 1, RegistryValueKind.DWord);
            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\AppHost", "EnableWebContentEvaluation");
            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\AppHost", "PreventOverride");
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

            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WinDefend", "Start", "2", RegistryValueKind.DWord, true);
            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WinDefend", "AutorunsDisabled");
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SecurityHealthService", "Start", "2", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows Defender\Features", "TamperProtection", "0", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\Sense", "Start", "3", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SgrmAgent", "Start", "3", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SgrmBroker", "Start", "3", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WdFilter", "Start", "0", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WdBoot", "Start", "0", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WdNisDrv", "Start", "3", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WdNisSvc", "Start", "3", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\MDCoreSvc", "Start", "3", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\MsSecCore", "Start", "3", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\MsSecFlt", "Start", "3", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\MsSecWfp", "Start", "3", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\webthreatdefsvc", "Start", "3", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\webthreatdefusersvc", "Start", "3", RegistryValueKind.DWord, true);

            foreach (var (filePath, originalFileName) in new (string filePath, string originalFileName)[]
            {
              (Path.Combine(StoragePaths.SystemDisk, "Windows", "System32", "BlockSS.exe"), "smartscreen.exe"),
              (Path.Combine(StoragePaths.SystemDisk, "Program Files", "Windows Defender", "BlockAntimalware.exe"), "MsMpEng.exe"),
              (Path.Combine(StoragePaths.SystemDisk, "Program Files", "Windows Defender", "BlockAntimalwareCore.exe"), "MpDefenderCoreService.exe")
            })
            {
                string newFilePath = Path.Combine(Path.GetDirectoryName(filePath), originalFileName);
                TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, $"cmd.exe /c rename \"{filePath}\" {originalFileName}");
                TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, $"cmd.exe /c icacls \"{newFilePath}\" /reset");
                TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, $"cmd.exe /c takeown /f \"{newFilePath}\" /a");
                TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, $"cmd.exe /c icacls \"{newFilePath}\" /setowner *S-1-5-80-956008885-3418522649-1831038044-1853292631-2271478464");
            }
            TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, $"cmd.exe /c for /d %D in (\"{Path.Combine(StoragePaths.SystemDisk, @"ProgramData\Microsoft\Windows Defender\Platform\*")}\") do if exist \"%D\\BlockAntimalware.exe\" ren \"%D\\BlockAntimalware.exe\" MsMpEng.exe");
            TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, $"cmd.exe /c for /d %D in (\"{Path.Combine(StoragePaths.SystemDisk, @"ProgramData\Microsoft\Windows Defender\Platform\*")}\") do if exist \"%D\\BlockAntimalwareCore.exe\" ren \"%D\\BlockAntimalwareCore.exe\" MpDefenderCoreService.exe");

            ManageServices(false);
            SetTaskState(true, WinDefenderTasks);
            RunPowerShellCommand(@"Get-AppXpackage Microsoft.WindowsDefender | Foreach {Add-AppxPackage -DisableDevelopmentMode -Register ""$($_.InstallLocation)\AppXManifest.xml""}");

            RegistryHelp.Write(Registry.ClassesRoot, @"*\shellex\ContextMenuHandlers\EPP", "", "{09A47860-11B0-4DA5-AFA5-26D86198A780}", RegistryValueKind.String);
            RegistryHelp.Write(Registry.ClassesRoot, @"Directory\shellex\ContextMenuHandlers\EPP", "", "{09A47860-11B0-4DA5-AFA5-26D86198A780}", RegistryValueKind.String);
            RegistryHelp.Write(Registry.ClassesRoot, @"Drive\shellex\ContextMenuHandlers\EPP", "", "{09A47860-11B0-4DA5-AFA5-26D86198A780}", RegistryValueKind.String);
            RegistryHelp.Write(Registry.ClassesRoot, @"CLSID\{09A47860-11B0-4DA5-AFA5-26D86198A780}\InprocServer32", "", StoragePaths.SystemDisk + @"Program Files\Windows Defender\shellext.dll", RegistryValueKind.String);
            RegistryHelp.Write(Registry.ClassesRoot, @"CLSID\{09A47860-11B0-4DA5-AFA5-26D86198A780}\InprocServer32", "ThreadingModel", "Apartment", RegistryValueKind.String);

            ImportRights();
        }

        internal static void Deactivate()
        {
            ExportRights();

            KillProcess(new[] { "smartscreen", "MpDefenderCoreService", "MsMpEng", "SecurityHealthService", "SecurityHealthSystray", "wuauserv", "SearchUI", "RuntimeBroker", "msedge", "ssoncom", "usocoreworker" });

            foreach (var (filePath, newFileName) in new (string filePath, string newFileName)[]
            {
                (Path.Combine(StoragePaths.SystemDisk, "Windows", "System32", "smartscreen.exe"), "BlockSS.exe"),
                (Path.Combine(StoragePaths.SystemDisk, "Program Files", "Windows Defender", "MsMpEng.exe"), "BlockAntimalware.exe"),
                (Path.Combine(StoragePaths.SystemDisk, "Program Files", "Windows Defender", "MpDefenderCoreService.exe"), "BlockAntimalwareCore.exe")
            })
            {
                TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, $"сmd.exe /c takeown /f \"{filePath}\"");
                TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, $"cmd.exe /c icacls \"{filePath}\" /inheritance:r /remove S-1-5-32-544 S-1-5-11 S-1-5-32-545 S-1-5-18");
                TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, $"cmd.exe /c icacls \"{filePath}\" /grant %username%:F");
                TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, $"cmd.exe /c rename \"{filePath}\" {newFileName}");
            }
            TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, $"cmd.exe /c for /d %D in (\"{Path.Combine(StoragePaths.SystemDisk, @"ProgramData\Microsoft\Windows Defender\Platform\*")}\") do if exist \"%D\\MsMpEng.exe\" ren \"%D\\MsMpEng.exe\" BlockAntimalware.exe");
            TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, $"cmd.exe /c for /d %D in (\"{Path.Combine(StoragePaths.SystemDisk, @"ProgramData\Microsoft\Windows Defender\Platform\*")}\") do if exist \"%D\\MpDefenderCoreService.exe\" ren \"%D\\MpDefenderCoreService.exe\" BlockAntimalwareCore.exe");

            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer", "SmartScreenEnabled", "Off", RegistryValueKind.String);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "EnableSmartScreen", 0, RegistryValueKind.DWord);

            TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, "cmd.exe /c reg add HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer /t REG_SZ /v SmartScreenEnabled /d \"off\" /f");

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

            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender", "DisableAntiSpyware", 1, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender", "DisableAntiVirus", 1, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender", "ServiceKeepAlive", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender", "AllowFastService", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender", "DisableRoutinelyTakingAction", 1, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender", "HideExclusionsFromLocalAdmins", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender", "ServiceKeepAlive", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender", "AllowFastServiceStartup", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender", "PUAProtection", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender", "DisableLocalAdminMerge", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows Defender", "PUAProtection", 0, RegistryValueKind.DWord);

            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\DomainProfile", "DisableNotifications", 1, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\PrivateProfile", "DisableNotifications", 1, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\StandardProfile", "DisableNotifications", 1, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender Security Center\Notifications", "DisableEnhancedNotifications", 1, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender Security Center\Notifications", "DisableNotifications", 1, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender Security Center\App and Browser protection", "DisallowExploitProtectionOverride", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender Security Center\App and Browser protection", "UILockdown", 1, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender\Features", "DeviceControlEnabled", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender\MpEngine", "MpEnablePus", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender\MpEngine", "DisableGradualRelease", 1, RegistryValueKind.DWord);

            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", "LocalSettingOverrideDisableOnAccessProtection", 0, RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", "LocalSettingOverrideRealtimeScanDirection", 0, RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", "LocalSettingOverrideDisableIOAVProtection", 0, RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", "LocalSettingOverrideDisableBehaviorMonitoring", 0, RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", "LocalSettingOverrideDisableIntrusionPreventionSystem", 0, RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", "LocalSettingOverrideDisableRealtimeMonitoring", 0, RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", "DisableBehaviorMonitoring", 1, RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", "DisableIOAVProtection", 1, RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", "DisableOnAccessProtection", 1, RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", "DisableRealtimeMonitoring", 1, RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", "DisableRoutinelyTakingAction", 1, RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", "DisableScanOnRealtimeEnable", 1, RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", "DisableScriptScanning", 1, RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", "DisableRawWriteNotification", 1, RegistryValueKind.DWord, true);

            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Microsoft Antimalware", "ServiceKeepAlive", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Microsoft Antimalware", "AllowFastServiceStartup", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Microsoft Antimalware", "DisableRoutinelyTakingAction", 1, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Microsoft Antimalware", "DisableAntiSpyware", 1, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Microsoft Antimalware", "DisableAntiVirus", 1, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Microsoft Antimalware\SpyNet", "SpyNetReporting", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Microsoft Antimalware\SpyNet", "LocalSettingOverrideSpyNetReporting", 0, RegistryValueKind.DWord);

            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender\Signature Updates", "SignatureDisableNotification", 1, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender\Signature Updates", "RealtimeSignatureDelivery", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender\Signature Updates", "ForceUpdateFromMU", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender\Signature Updates", "DisableScheduledSignatureUpdateOnBattery", 1, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender\Signature Updates", "UpdateOnStartUp", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender\Signature Updates", "SignatureUpdateCatchupInterval", 2, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender\Signature Updates", "DisableUpdateOnStartupWithoutEngine", 1, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender\Signature Updates", "DisableScanOnUpdate", 1, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender\Signature Updates", "ScheduleTime", 5184, RegistryValueKind.DWord);

            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Security Center", "FirstRunDisabled", 1, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Security Center", "AntiVirusOverride", 1, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Security Center", "FirewallOverride", 1, RegistryValueKind.DWord);

            RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\UX Configuration", "Notification_Suppress", 1, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\UX Configuration", "SuppressRebootNotification", 1, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\UX Configuration", "UILockdown", 1, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender Security Center\Device performance and health", "UILockdown", 1, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender Security Center\Family optionsObj", "UILockdown", 1, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender\Reportin", "DisableEnhancedNotifications", 1, RegistryValueKind.DWord);

            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\WMI\Autologger\DefenderApiLogger", "Start", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\WMI\Autologger\DefenderAuditLogger", "Start", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\WINEVT\Channels\Microsoft-Windows-Windows Defender/Operational", "Enabled", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\WINEVT\Channels\Microsoft-Windows-Security-Diagnostic/Operational", "Enabled", 0, RegistryValueKind.DWord);

            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows Defender\Cloud", "DisableBlockAtFirstSeen", 1, RegistryValueKind.DWord); 
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows Defender\Cloud", "CaptureBetaFeatures", 0, RegistryValueKind.DWord);

            RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\SpyNet", "DisableBlockAtFirstSeen", 1, RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\SpyNet", "SpynetReporting", 0, RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\SpyNet", "SubmitSamplesConsent", 2, RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows Defender\SpyNet", "LocalSettingOverrideSpynetReporting", 0, RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\CI\Config", "EnabledV9", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\CI\Config", "VulnerableDriverBlocklistEnable", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\DeviceGuard\Scenarios\HypervisorEnforcedCodeIntegrity", "Enabled", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\DeviceGuard", "EnableVirtualizationBasedSecurity", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WTDS\Components", "ServiceEnabled", "0", RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\MicrosoftEdge\PhishingFilter", "EnabledV9", "0", RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\AppHost", "EnableWebContentEvaluation", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\AppHost", "PreventOverride", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppContainer\Storage\microsoft.microsoftedge_8wekyb3d8bbwe\MicrosoftEdge", "EnabledV9", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppContainer\Storage\microsoft.microsoftedge_8wekyb3d8bbwe\MicrosoftEdge", "PreventOverride", 0, RegistryValueKind.DWord);

            SetTaskState(false, WinDefenderTasks);

            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run", "SecurityHealth", new byte[] { 0099, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99 }, RegistryValueKind.Binary);
            RegistryHelp.DeleteValue(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "Windows Defender");
            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "SecurityHealth");

            KillProcess(new[] { "SecurityHealthService", "SecurityHealthSystray" });

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
                "reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\SmartScreen\" /t REG_SZ /v ConfigureAppInstallControl /d \"Anywhere\" /f & " +
                "reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\SmartScreen\" /t REG_DWORD /v ConfigureAppInstallControlEnabled /d 0 /f & " +
                "reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\WinDefend /t REG_DWORD /v AutorunsDisabled /d 3 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Image File Execution Options\\MsMpEng.exe\" /t REG_DWORD /v CFGOptions /d 0 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Image File Execution Options\\MsMpEng.exe\" /t REG_SZ /v Debugger /d \"S:\\none.exe\" /f & " +
                "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Image File Execution Options\\SgrmBroker.exe\" /t REG_DWORD /v CFGOptions /d 0 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Image File Execution Options\\SgrmBroker.exe\" /t REG_SZ /v Debugger /d \"S:\\none.exe\" /f & " +
                "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Image File Execution Options\\MpDefenderCoreService.exe\" /t REG_DWORD /v CFGOptions /d 0 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Image File Execution Options\\MpDefenderCoreService.exe\" /t REG_SZ /v Debugger /d \"S:\\none.exe\" /f & " +
                "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Image File Execution Options\\smartscreen.exe\" /t REG_DWORD /v CFGOptions /d 0 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Image File Execution Options\\smartscreen.exe\" /t REG_SZ /v Debugger /d \"S:\\none.exe\" /f");

            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WinDefend", "Start", "4", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WinDefend", "AutorunsDisabled", "3", RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SecurityHealthService", "Start", "4", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows Defender\Features", "TamperProtection", "0", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\Sense", "Start", "4", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SgrmAgent", "Start", "4", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SgrmBroker", "Start", "4", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WdFilter", "Start", "4", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WdBoot", "Start", "4", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WdNisDrv", "Start", "4", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WdNisSvc", "Start", "4", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\MDCoreSvc", "Start", "4", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\MsSecCore", "Start", "4", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\MsSecFlt", "Start", "4", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\MsSecWfp", "Start", "4", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\webthreatdefsvc", "Start", "4", RegistryValueKind.DWord, true);
            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\webthreatdefusersvc", "Start", "4", RegistryValueKind.DWord, true);

            RegistryHelp.DeleteFolderTree(Registry.ClassesRoot, @"*\shellex\ContextMenuHandlers\EPP");
            RegistryHelp.DeleteFolderTree(Registry.ClassesRoot, @"Directory\shellex\ContextMenuHandlers\EPP");
            RegistryHelp.DeleteFolderTree(Registry.ClassesRoot, @"Drive\shellex\ContextMenuHandlers\EPP");
            RegistryHelp.DeleteFolderTree(Registry.ClassesRoot, @"CLSID\{09A47860-11B0-4DA5-AFA5-26D86198A780}\InprocServer32");

            ManageServices(true);

            foreach (var directory in new[]
            {
                Path.Combine(StoragePaths.SystemDisk, "ProgramData", "Microsoft", "Windows Defender", "Scans", "History"),
                Path.Combine(StoragePaths.SystemDisk, "ProgramData", "Microsoft", "Windows Defender", "Scans", "Cache"),
                Path.Combine(StoragePaths.SystemDisk, "ProgramData", "Microsoft", "Windows Defender", "Support")
            })
            {
                try
                {
                    if (!Directory.Exists(directory))
                        continue;

                    foreach (var filePath in Directory.EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly))
                        TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, $"cmd.exe /c takeown /f \"{filePath}\" & icacls \"{filePath}\" /inheritance:r /remove S-1-5-32-544 S-1-5-11 S-1-5-32-545 S-1-5-18 & icacls \"{filePath}\" /grant %username%:F & del /q \"{filePath}\"");
                }
                catch (Exception ex) { ErrorLogging.LogDebug(ex); }
            }
        }

        private static void ManageServices(bool isDisable)
        {
            Dictionary<string, (int onValue, int offValue)> services = new Dictionary<string, (int onValue, int offValue)>
            {
                { "WinDefend", (2, 4) },
                { "SecurityHealthService", (2, 4) },
                { "Sense", (3, 4) },
                { "SgrmAgent", (3, 4) },
                { "SgrmBroker", (3, 4) },
                { "WdBoot", (0, 4) },
                { "WdFilter", (0, 4) },
                { "WdNisDrv", (3, 4) },
                { "WdNisSvc", (3, 4) },
                { "MDCoreSvc", (3, 4) },
                { "MsSecCore", (3, 4) },
                { "MsSecFlt", (3, 4) },
                { "MsSecWfp", (3, 4) },
                { "webthreatdefsvc", (3, 4) },
                { "webthreatdefusersvc", (3, 4) },
                { "wscsvc", (3, 4) }
            };

            foreach (var service in services)
            {
                if (isDisable)
                    RunPowerShellCommand($"Stop-Service -Name {service.Key}; Set-Service -Name {service.Key} -StartupType Disabled");
                else
                    RunPowerShellCommand($"Start-Service -Name {service.Key}; Set-Service -Name {service.Key} -StartupType Automatic");
                RunCmdCommand($"sc config {service.Key} start= {(isDisable ? "disabled" : "auto")}");
                RunPowerShellCommand($@"Set-ExecutionPolicy UnRestricted; Set-ItemProperty -Path HKLM:\SYSTEM\CurrentControlSet\Services\{service} -Name Start -Value 0x0000000{(isDisable ? service.Value.offValue : service.Value.onValue)}");
                TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsRepository.PID, $"cmd.exe /c reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\{service} /t REG_DWORD /v Start /d {(isDisable ? service.Value.offValue : service.Value.onValue)} /f");
            }
        }

        private static void KillProcess(params string[] nameList)
        {
            TakingOwnership.GrantDebugPrivilege();
            foreach (string getName in nameList)
            {
                foreach (Process process in Process.GetProcessesByName(getName))
                {
                    try
                    {
                        process.Kill();
                        process.WaitForExit(1000);
                    }
                    catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                }
            }
        }

        private static void RunPowerShellCommand(string command)
        {
            RunProcess(Path.Combine(Environment.SystemDirectory, @"WindowsPowerShell\v1.0\powershell.exe"),
                "-NoLogo -NonInteractive -NoProfile -ExecutionPolicy Bypass -EncodedCommand \"" +
                Convert.ToBase64String(Encoding.Unicode.GetBytes(command)) + "\"");
        }

        private static void RunCmdCommand(string command) => RunProcess(Path.Combine(Environment.SystemDirectory, "cmd.exe"), "/d /q /c " + command);

        private static void RunProcess(string path, string arguments)
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
            catch (Exception ex) { ErrorLogging.LogDebug(ex); }
        }
    }
}

