using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using Microsoft.Win32;

namespace GTweak.Utilities.Tweaks.DefenderManager
{
    internal class WindowsDefender : BackupRights
    {
        private static readonly Dictionary<string, string> services = new Dictionary<string, string>
        {
            { "WinDefend", "2" },
            { "SecurityHealthService", "2"},
            { "Sense", "3" },
            { "SgrmAgent", "3" },
            { "SgrmBroker", "3" },
            { "MDCoreSvc", "3" },
            { "MsSecCore", "3" },
            { "MsSecFlt", "3" },
            { "MsSecWfp", "3" },
            { "WdFilter", "0" },
            { "WdBoot", "0" },
            { "WdNisDrv", "3" },
            { "WdNisSvc", "3"},
            { "webthreatdefsvc", "3" },
            { "webthreatdefusersvc", "3" },
            { "wscsvc", "3" }
        };

        private static readonly (string Path, string Normal, string Block)[] fileMappings = new (string Path, string Normal, string Block)[]
        {
           (Environment.SystemDirectory, "smartscreen.exe", "BlockSS.exe"),
           (PathLocator.Folders.WindowsDefender, "MsMpEng.exe", "BlockAntimalware.exe"),
           (PathLocator.Folders.WindowsDefender, "MpDefenderCoreService.exe", "BlockAntimalwareCore.exe"),
           (PathLocator.Folders.WindowsDefender, "MpCmdRun.exe", "BlockMpCmdRun.exe"),
           (PathLocator.Folders.WindowsDefender, "MpCopyAccelerator.exe", "BlockMpCopyAccelerator.exe"),
           (PathLocator.Folders.WindowsDefender, "DlpUserAgent.exe", "BlockDlpUserAgent.exe"),
           (PathLocator.Folders.WindowsDefender, "MpDlpCmd.exe", "BlockMpDlpCmd.exe"),
           (PathLocator.Folders.WindowsDefender, "MpDlpService.exe", "BlockMpDlpService.exe"),
           (PathLocator.Folders.WindowsDefender, "mpextms.exe", "Blockmpextms.exe"),
           (PathLocator.Folders.WindowsDefender, "NisSrv.exe", "BlockNisSrv.exe"),
           (PathLocator.Folders.WindowsDefender, "ConfigSecurityPolicy.exe", "BlockConfigSecurityPolicy.exe"),
           (Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),"Windows Defender","BlockWindowsDefenderX86"),
           (Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Windows Defender Advanced Threat Protection", "BlockWindowsDefenderATP" ),
            (Path.Combine(Environment.SystemDirectory, "HealthAttestationClient"), "HealthAttestationClientAgent.exe", "BlockHACA.exe")
        };


        internal static void SetProtectionState(bool isDisabled)
        {
            if (isDisabled)
            {
                Deactivate();
            }
            else
            {
                Activate();
            }
        }

        private static void Activate()
        {
            KillProcess();

            foreach (var (path, normal, block) in fileMappings)
            {
                CommandExecutor.RunCommandAsTrustedInstaller($"{PathLocator.Executable.NSudo} -U:T -P:E -ShowWindowMode:Hide -Wait cmd /c rename \"{Path.Combine(path, block)}\" {normal}");
            }

            CommandExecutor.RunCommandAsTrustedInstaller($"{PathLocator.Executable.NSudo} -U:T -P:E -ShowWindowMode:Hide -Wait cmd /c for /d %D in (\"{Path.Combine(PathLocator.Folders.SystemDrive, @"ProgramData\Microsoft\Windows Defender\Platform\*")}\") do if exist \"%D\\BlockAntimalware.exe\" ren \"%D\\BlockAntimalware.exe\" MsMpEng.exe");
            CommandExecutor.RunCommandAsTrustedInstaller($"{PathLocator.Executable.NSudo} -U:T -P:E -ShowWindowMode:Hide -Wait cmd /c for /d %D in (\"{Path.Combine(PathLocator.Folders.SystemDrive, @"ProgramData\Microsoft\Windows Defender\Platform\*")}\") do if exist \"%D\\BlockAntimalwareCore.exe\" ren \"%D\\BlockAntimalwareCore.exe\" MpDefenderCoreService.exe");

            CommandExecutor.RunCommandAsTrustedInstaller("/c reg delete \"HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\AppHost\" /v EnableWebContentEvaluation /f & " +
                "reg delete \"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\" /v SmartScreenEnabled /f & " +
                "reg delete \"HKLM\\SOFTWARE\\Policies\\Microsoft\\MicrosoftEdge\\PhishingFilter\" /v EnabledV9 /f & " +
                "reg delete \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\System\" /v EnableSmartScreen /f & " +
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
                "reg delete \"HKLM\\SOFTWARE\\Policies\\Microsoft\\MRT\" /v DontOfferThroughWUA /f & " +
                "reg delete \"HKLM\\SOFTWARE\\Policies\\Microsoft\\MRT\" /v DontReportInfectionInformation /f & " +
                "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\" /v PUAProtection /t REG_DWORD /d 2 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Spynet\" /v SpyNetReporting /t REG_DWORD /d 1 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Spynet\" /v SubmitSamplesConsent /t REG_DWORD /d 0 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\\Features\" /v TamperProtection /t REG_DWORD /d 0 /f");

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

            RunPowerShellCommand(@"Set-ExecutionPolicy UnRestricted; Set-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows Defender\Real-Time Protection' -Name DisableBehaviorMonitoring -Value 0x00000000");
            RunPowerShellCommand(@"Set-ExecutionPolicy UnRestricted; Set-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows Defender\Real-Time Protection' -Name DisableIOAVProtection -Value 0x00000000");
            RunPowerShellCommand(@"Set-ExecutionPolicy UnRestricted; Set-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows Defender\Real-Time Protection' -Name DisableOnAccessProtection -Value 0x00000000");
            RunPowerShellCommand(@"Set-ExecutionPolicy UnRestricted; Set-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows Defender\Real-Time Protection' -Name DisableRealtimeMonitoring -Value 0x00000000");
            RunPowerShellCommand(@"Set-ExecutionPolicy UnRestricted; Set-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows Defender\Real-Time Protection' -Name DisableScanOnRealtimeEnable -Value 0x00000000");
            RunPowerShellCommand(@"Set-ExecutionPolicy UnRestricted; Set-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows Defender\Real-Time Protection' -Name DisableScriptScanning -Value 0x00000000");
            RunPowerShellCommand(@"Set-ExecutionPolicy UnRestricted; Set-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows Defender\Real-Time Protection' -Name DisableScriptScanning -Value 0x00000000");

            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run", "SecurityHealth", new byte[] { 0002, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 }, RegistryValueKind.Binary);
            RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "Windows Defender", "\"%ProgramFiles%\\Windows Defender\\MSASCui.exe\"-runkey", RegistryValueKind.String);

            ManageExclusions(false);

            RegistryHelp.Write(Registry.ClassesRoot, @"*\shellex\ContextMenuHandlers\EPP", "", "{09A47860-11B0-4DA5-AFA5-26D86198A780}", RegistryValueKind.String);
            RegistryHelp.Write(Registry.ClassesRoot, @"Directory\shellex\ContextMenuHandlers\EPP", "", "{09A47860-11B0-4DA5-AFA5-26D86198A780}", RegistryValueKind.String);
            RegistryHelp.Write(Registry.ClassesRoot, @"Drive\shellex\ContextMenuHandlers\EPP", "", "{09A47860-11B0-4DA5-AFA5-26D86198A780}", RegistryValueKind.String);
            RegistryHelp.Write(Registry.ClassesRoot, @"CLSID\{09A47860-11B0-4DA5-AFA5-26D86198A780}\InprocServer32", "", PathLocator.Folders.SystemDrive + @"Program Files\Windows Defender\shellext.dll", RegistryValueKind.String);
            RegistryHelp.Write(Registry.ClassesRoot, @"CLSID\{09A47860-11B0-4DA5-AFA5-26D86198A780}\InprocServer32", "ThreadingModel", "Apartment", RegistryValueKind.String);

            foreach (var service in services)
            {
                CommandExecutor.RunCommandAsTrustedInstaller($@"{PathLocator.Executable.NSudo} -U:T -P:E -ShowWindowMode:Hide -Wait cmd /c reg add HKLM\SYSTEM\CurrentControlSet\Services\{service.Key} /v Start /t REG_DWORD /d {service.Value} /f");
            }

            CommandExecutor.RunCommandAsTrustedInstaller($@"{PathLocator.Executable.NSudo} -U:T -P:E -ShowWindowMode:Hide -Wait cmd /c reg add HKLM\SYSTEM\CurrentControlSet\Services\WinDefend /v AutorunsDisabled /t REG_DWORD /d 0 /f");
            CommandExecutor.RunCommandAsTrustedInstaller($@"/c reg add HKLM\SYSTEM\CurrentControlSet\Services\wscsvc /v DelayedAutoStart /t REG_DWORD /d 0 /f");
            CommandExecutor.RunCommandAsTrustedInstaller($@"{PathLocator.Executable.NSudo} -U:T -P:E -ShowWindowMode:Hide -Wait cmd /c reg add ""HKLM\SYSTEM\CurrentControlSet\Services\WdFilter\Instances\WdFilter Instance"" /v Altitude /t REG_SZ /d 328010 /f");

            SetTaskState(true, winDefenderTasks);
            RunPowerShellCommand(@"Get-AppXpackage Microsoft.WindowsDefender | Foreach {Add-AppxPackage -DisableDevelopmentMode -Register ""$($_.InstallLocation)\AppXManifest.xml""}");

            ImportRights();
        }

        private static void Deactivate()
        {
            ExportRights();

            KillProcess();

            foreach (var (path, normal, block) in fileMappings)
            {
                CommandExecutor.RunCommandAsTrustedInstaller($"{PathLocator.Executable.NSudo} -U:T -P:E -ShowWindowMode:Hide -Wait cmd /c rename \"{Path.Combine(path, normal)}\" \"{block}\"");
            }

            foreach (var service in services)
            {
                CommandExecutor.RunCommandAsTrustedInstaller($@"{PathLocator.Executable.NSudo} -U:T -P:E -ShowWindowMode:Hide -Wait cmd /c reg add HKLM\SYSTEM\CurrentControlSet\Services\{service.Key} /v Start /t REG_DWORD /d 4 /f");
            }

            CommandExecutor.RunCommandAsTrustedInstaller($"{PathLocator.Executable.NSudo} -U:T -P:E -ShowWindowMode:Hide -Wait cmd /c for /d %D in (\"{Path.Combine(PathLocator.Folders.SystemDrive, @"ProgramData\Microsoft\Windows Defender\Platform\*")}\") do if exist \"%D\\MsMpEng.exe\" ren \"%D\\MsMpEng.exe\" BlockAntimalware.exe");
            CommandExecutor.RunCommandAsTrustedInstaller($"{PathLocator.Executable.NSudo} -U:T -P:E -ShowWindowMode:Hide -Wait cmd /c for /d %D in (\"{Path.Combine(PathLocator.Folders.SystemDrive, @"ProgramData\Microsoft\Windows Defender\Platform\*")}\") do if exist \"%D\\MpDefenderCoreService.exe\" ren \"%D\\MpDefenderCoreService.exe\" BlockAntimalwareCore.exe");

            CommandExecutor.RunCommandAsTrustedInstaller($@"{PathLocator.Executable.NSudo} -U:T -P:E -ShowWindowMode:Hide -Wait cmd /c reg add HKLM\SYSTEM\CurrentControlSet\Services\WinDefend /v AutorunsDisabled /t REG_DWORD /d 3 /f");
            CommandExecutor.RunCommandAsTrustedInstaller($@"/c reg add HKLM\SYSTEM\CurrentControlSet\Services\wscsvc /v DelayedAutoStart /t REG_DWORD /d 1 /f");
            CommandExecutor.RunCommandAsTrustedInstaller($@"{PathLocator.Executable.NSudo} -U:T -P:E -ShowWindowMode:Hide -Wait cmd /c reg delete ""HKLM\SYSTEM\CurrentControlSet\Services\WdFilter\Instances\WdFilter Instance"" /v Altitude /f");

            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer", "SmartScreenEnabled", "Off", RegistryValueKind.String);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "EnableSmartScreen", 0, RegistryValueKind.DWord);

            CommandExecutor.RunCommandAsTrustedInstaller("/c reg add HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer /v SmartScreenEnabled /t REG_SZ /d \"off\" /f");

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

            SetTaskState(false, winDefenderTasks);

            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run", "SecurityHealth", new byte[] { 0099, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99 }, RegistryValueKind.Binary);
            RegistryHelp.DeleteValue(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "Windows Defender");
            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "SecurityHealth");



            CommandExecutor.RunCommandAsTrustedInstaller($"/c reg add \"HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\AppHost\" /v EnableWebContentEvaluation /t REG_DWORD /d 0 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\" /v SmartScreenEnabled /t REG_SZ /d \"off\" /f & " +
                "reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\MicrosoftEdge\\PhishingFilter\" /v EnabledV9 /t REG_DWORD /d 0 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\System\" /v EnableSmartScreen /t REG_DWORD /d 0 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\" /v DisableAntiSpyware /t REG_DWORD /d 1 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\" /v DisableAntiVirus /t REG_DWORD /d 1 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\\Real-Time Protection\" /v LocalSettingOverrideDisableOnAccessProtection /t REG_DWORD /d 0 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\\Real-Time Protection\" /v LocalSettingOverrideRealtimeScanDirection /t REG_DWORD /d 0 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\\Real-Time Protection\" /v LocalSettingOverrideDisableIOAVProtection /t REG_DWORD /d 0 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\\Real-Time Protection\" /v LocalSettingOverrideDisableBehaviorMonitoring /t REG_DWORD /d 0 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\\Real-Time Protection\" /v LocalSettingOverrideDisableIntrusionPreventionSystem /t REG_DWORD /d 0 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\\Real-Time Protection\" /v LocalSettingOverrideDisableRealtimeMonitoring /t REG_DWORD /d 0 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\\Real-Time Protection\" /v DisableBehaviorMonitoring /t REG_DWORD /d 1 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\\Real-Time Protection\" /v DisableOnAccessProtection /t REG_DWORD /d 1 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\\Real-Time Protection\" /v DisableIOAVProtection /t REG_DWORD /d 1 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\\Real-Time Protection\" /v DisableScanOnRealtimeEnable /t REG_DWORD /d 1 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\\Real-Time Protection\" /v DisableRealtimeMonitoring /t REG_DWORD /d 1 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Spynet\" /v SpyNetReporting /t REG_DWORD /d 0 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Spynet\" /v SubmitSamplesConsent /t REG_DWORD /d 2 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\" /v PUAProtection /t REG_DWORD /d 0 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows Defender\\Features\" /v TamperProtection /t REG_DWORD /d 0 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\MRT\" /v DontOfferThroughWUA /t REG_DWORD /d 1 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\MRT\" /v DontReportInfectionInformation /t REG_DWORD /d 1 /f & " +
                "reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\SmartScreen\" /v ConfigureAppInstallControl /t REG_SZ /d \"Anywhere\" /f & " +
                "reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\SmartScreen\" /v ConfigureAppInstallControlEnabled /t REG_DWORD /d 0 /f");

            ManageExclusions(true);

            RegistryHelp.DeleteFolderTree(Registry.ClassesRoot, @"*\shellex\ContextMenuHandlers\EPP");
            RegistryHelp.DeleteFolderTree(Registry.ClassesRoot, @"Directory\shellex\ContextMenuHandlers\EPP");
            RegistryHelp.DeleteFolderTree(Registry.ClassesRoot, @"Drive\shellex\ContextMenuHandlers\EPP");
            RegistryHelp.DeleteFolderTree(Registry.ClassesRoot, @"CLSID\{09A47860-11B0-4DA5-AFA5-26D86198A780}\InprocServer32");

            CommandExecutor.RunCommand("Get-PSDrive -PSProvider FileSystem | ForEach-Object { Add-MpPreference -ExclusionPath \"$($_.Root)\" } ", true);

            foreach (var directory in new[]
            {
                Path.Combine(PathLocator.Folders.SystemDrive, "ProgramData", "Microsoft", "Windows Defender", "Scans", "History"),
                Path.Combine(PathLocator.Folders.SystemDrive, "ProgramData", "Microsoft", "Windows Defender", "Scans", "Workspace"),
                Path.Combine(PathLocator.Folders.SystemDrive, "ProgramData", "Microsoft", "Windows Defender", "Support")
            })
            {
                try
                {
                    if (!Directory.Exists(directory))
                    {
                        continue;
                    }

                    foreach (var filePath in Directory.EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly))
                    {
                        CommandExecutor.RunCommandAsTrustedInstaller($"{PathLocator.Executable.NSudo} -U:T -P:E -ShowWindowMode:Hide -Wait cmd /c del /q \"{filePath}\"");
                    }
                }
                catch (Exception ex) { ErrorLogging.LogDebug(ex); }
            }
        }

        private static void ManageExclusions(bool isDisable)
        {
            foreach (string drive in DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed || d.DriveType == DriveType.Removable).Select(d => d.Name))
            {
                CommandExecutor.RunCommand($" {(isDisable ? "Add-MpPreference" : "Remove-MpPreference")} -ExclusionPath '{drive}'", true);
            }
        }

        private static void KillProcess()
        {
            string[] processes = { "smartscreen", "mpdefendercoreservice", "msmpeng", "securityhealthservice", "securityhealthsystray", "securityhealthui", "wuauserv", "searchui", "sechealthui", "runtimebroker", "msedge", "ssoncom", "usocoreworker", "defenderbootstrapper", "configsecuritypolicy", "dlpuseragent", "lsass", "mpam-d", "mpam-fe", "mpam-fe_bd", "mpas-d", "mpas-fe", "mpas-fe_bd", "mpav-d", "mpav-fe", "mpav-fe_bd", "mpcmdrun", "mpcopyaccelerator", "mpdlpcmd", "mpdlpservice", "mpextms", "mpsigstub", "mrt", "msmpengcp", "mssense", "nissrv", "offlinescannershell", "securekernel", "securityhealthhost", "senseap", "senseaptoast", "sensecm", "sensegpparser", "senseidentity", "senseimdscollector", "senseir", "sensendr", "sensesampleuploader", "sensetvm", "sensece", "sgrmbroker", "healthattestationclientagent" };
            CommandExecutor.RunCommandAsTrustedInstaller($"/c taskkill /f " + string.Join(" ", processes.Select(p => $"/im {p}.exe")));
        }

        private static void RunPowerShellCommand(string command) => RunProcess(PathLocator.Executable.PowerShell, "-NoLogo -NonInteractive -NoProfile -ExecutionPolicy Bypass -EncodedCommand \"" + Convert.ToBase64String(Encoding.Unicode.GetBytes(command)) + "\"");

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

