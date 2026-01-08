using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
            TerminateProcess();

            InvokePowerShell(@"Set-MpPreference -DisableIOAVProtection $false
            Set-MpPreference -DisableRealtimeMonitoring $false
            Set-MpPreference -DisableBehaviorMonitoring $false
            Set-MpPreference -DisableBlockAtFirstSeen $false
            Set-MpPreference -DisablePrivacyMode $false
            Set-MpPreference -SignatureDisableUpdateOnStartupWithoutEngine $false
            Set-MpPreference -DisableArchiveScanning $false
            Set-MpPreference -DisableIntrusionPreventionSystem $false
            Set-MpPreference -DisableScriptScanning $false
            Set-MpPreference -SubmitSamplesConsent 1
            Set-MpPreference -MAPSReporting 2
            Set-MpPreference -PUAProtection Enabled");

            CommandExecutor.RunCommandAsTrustedInstaller($"{PathLocator.Executable.NSudo} -U:T -P:E -ShowWindowMode:Hide -Wait cmd /c " +
            @"reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"" /v LocalSettingOverrideDisableOnAccessProtection /f & " +
            @"reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"" /v LocalSettingOverrideRealtimeScanDirection /f & " +
            @"reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"" /v LocalSettingOverrideDisableIOAVProtection /f & " +
            @"reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"" /v LocalSettingOverrideDisableBehaviorMonitoring /f & " +
            @"reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"" /v LocalSettingOverrideDisableIntrusionPreventionSystem /f & " +
            @"reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"" /v LocalSettingOverrideDisableRealtimeMonitoring /f & " +
            @"reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"" /v DisableBehaviorMonitoring /f & " +
            @"reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"" /v DisableIOAVProtection /f & " +
            @"reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"" /v DisableOnAccessProtection /f & " +
            @"reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"" /v DisableRealtimeMonitoring /f & " +
            @"reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"" /v DisableRoutinelyTakingAction /f & " +
            @"reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"" /v DisableScanOnRealtimeEnable /f & " +
            @"reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"" /v DisableScriptScanning /f & " +
            @"reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"" /v DisableRawWriteNotification /f & " +
            @"reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"" /v DisableIntrusionPreventionSystem /f & " +
            @"reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"" /v DisableArchiveScanning /f & " +
            @"reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"" /v SignatureDisableUpdateOnStartupWithoutEngine /f & " +
            @"reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"" /v DisablePrivacyMode /f & " +
            @"reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"" /v DisableBlockAtFirstSeen /f & " +
            @"reg delete ""HKCU\Software\Microsoft\Windows\CurrentVersion\AppHost"" /v EnableWebContentEvaluation /t REG_DWORD /d 0 /f & " +
            @"reg delete ""HKLM\SOFTWARE\Policies\Microsoft\MicrosoftEdge\PhishingFilter"" /v EnabledV9 /f & " +
            @"reg add ""HKLM\SOFTWARE\Microsoft\Windows Defender"" /v PUAProtection /t REG_DWORD /d 2 /f & " +
            @"reg add ""HKLM\SOFTWARE\Microsoft\Windows Defender\Features"" /v TamperProtection /t REG_DWORD /d 1 /f & " +
            @"reg delete ""HKLM\SOFTWARE\Policies\Microsoft\MRT"" /v DontOfferThroughWUA /f & " +
            @"reg delete ""HKLM\SOFTWARE\Policies\Microsoft\MRT"" /v DontReportInfectionInformation /f & " +
            @"reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\SmartScreen"" /v ConfigureAppInstallControl /f & " +
            @"reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\SmartScreen"" /v DontReportInfectionInformation /f & " +
            @"reg add ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\WINEVT\Channels\Microsoft-Windows-DeviceConfidence/Analytic"" /v Enabled /t REG_DWORD /d 1 /f & " +
            @"reg add ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\WINEVT\Channels\Microsoft-Windows-DeviceGuard/Operational"" /v Enabled /t REG_DWORD /d 1 /f & " +
            @"reg add ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\WINEVT\Channels\Microsoft-Windows-DeviceGuard/Verbose"" /v Enabled /t REG_DWORD /d 1 /f");

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

            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run", "SecurityHealth", new byte[] { 0002, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 }, RegistryValueKind.Binary);
            RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "Windows Defender", "\"%ProgramFiles%\\Windows Defender\\MSASCui.exe\"-runkey", RegistryValueKind.String);

            RegistryHelp.Write(Registry.ClassesRoot, @"*\shellex\ContextMenuHandlers\EPP", "", "{09A47860-11B0-4DA5-AFA5-26D86198A780}", RegistryValueKind.String);
            RegistryHelp.Write(Registry.ClassesRoot, @"Directory\shellex\ContextMenuHandlers\EPP", "", "{09A47860-11B0-4DA5-AFA5-26D86198A780}", RegistryValueKind.String);
            RegistryHelp.Write(Registry.ClassesRoot, @"Drive\shellex\ContextMenuHandlers\EPP", "", "{09A47860-11B0-4DA5-AFA5-26D86198A780}", RegistryValueKind.String);
            RegistryHelp.Write(Registry.ClassesRoot, @"CLSID\{09A47860-11B0-4DA5-AFA5-26D86198A780}\InprocServer32", "", PathLocator.Folders.SystemDrive + @"Program Files\Windows Defender\shellext.dll", RegistryValueKind.String);
            RegistryHelp.Write(Registry.ClassesRoot, @"CLSID\{09A47860-11B0-4DA5-AFA5-26D86198A780}\InprocServer32", "ThreadingModel", "Apartment", RegistryValueKind.String);

            ManageExclusions(false);

            CommandExecutor.RunCommandAsTrustedInstaller($@"{PathLocator.Executable.NSudo} -U:T -P:E -ShowWindowMode:Hide -Wait cmd /c " +
            @"reg add HKLM\SYSTEM\CurrentControlSet\Services\WinDefend /v AutorunsDisabled /t REG_DWORD /d 0 /f & " +
            @"reg add HKLM\SYSTEM\CurrentControlSet\Services\wscsvc /v DelayedAutoStart /t REG_DWORD /d 0 /f & " +
            @"reg add ""HKLM\SYSTEM\CurrentControlSet\Services\WdFilter\Instances\WdFilter Instance"" /v Altitude /t REG_SZ /d 328010 /f & " +
            $"for /d %D in (\"{Path.Combine(PathLocator.Folders.SystemDrive, @"ProgramData\\Microsoft\\Windows Defender\\Platform\\*")}\") do if exist \"%D\\BlockAntimalware.exe\" ren \"%D\\BlockAntimalware.exe\" MsMpEng.exe & " +
            $"for /d %D in (\"{Path.Combine(PathLocator.Folders.SystemDrive, @"ProgramData\\Microsoft\\Windows Defender\\Platform\\*")}\") do if exist \"%D\\BlockAntimalwareCore.exe\" ren \"%D\\BlockAntimalwareCore.exe\" MpDefenderCoreService.exe & " +
            @"reg delete ""HKLM\Software\Policies\Microsoft\Windows Defender\SpyNet"" /v DisableBlockAtFirstSeen /f & " +
            @"reg add ""HKLM\SOFTWARE\Microsoft\Windows Defender\Windows Defender\ControlledFolderAccess"" /v Enable /t REG_DWORD /d 1 /f & " +
            @"reg add ""HKLM\SOFTWARE\Microsoft\Windows defender\Windows defender Exploit Guard\ASR"" /v EnableASRConsumers /t REG_DWORD /d 1/f & " +
            @"reg add ""HKLM\SOFTWARE\Microsoft\Windows defender\Windows defender Exploit Guard\Controlled Folder Access"" /v EnableControlledFolderAccess /t REG_DWORD /d 1 /f & " +
            @"reg add ""HKLM\SOFTWARE\Microsoft\Windows defender\Windows defender Exploit Guard\Network Protection"" /v EnableNetworkProtection /t REG_DWORD /d 1 /f & " +
            @"reg add ""HKLM\SOFTWARE\Microsoft\Windows defender\CoreService"" /v DisableCoreService1DSTelemetry /t REG_DWORD /d 0 /f & " +
            @"reg add ""HKLM\SOFTWARE\Microsoft\Windows defender\CoreService"" /v DisableCoreServiceECSIntegration /t REG_DWORD /d 0 /f & " +
            @"reg add ""HKLM\SOFTWARE\Microsoft\Windows defender\CoreService"" /v MdDisableResController /t REG_DWORD /d 0 /f & " +
            @"reg add ""HKLM\Software\Policies\Microsoft\Windows Defender\SpyNet"" /v SpynetReporting /t REG_DWORD /d 1 /f & " +
            @"reg add ""HKLM\Software\Policies\Microsoft\Windows Defender\SpyNet"" /v SubmitSamplesConsent /t REG_DWORD /d 0 /f & " +
            @"reg delete ""HKLM\Software\Policies\Microsoft\Windows Defender\SpyNet"" /v LocalSettingOverrideSpynetReporting /f & " +
            @"reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender"" /v DisableAntiSpyware /t REG_DWORD /d 0 /f & " +
            @"reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender"" /v DisableAntiVirus /t REG_DWORD /d 0 /f");

            ImportRights();

            foreach (var (path, normal, block) in fileMappings)
            {
                CommandExecutor.RunCommandAsTrustedInstaller($"{PathLocator.Executable.NSudo} -U:T -P:E -ShowWindowMode:Hide -Wait cmd /c rename \"{Path.Combine(path, block)}\" {normal}");
            }
            foreach (var service in services)
            {
                CommandExecutor.RunCommandAsTrustedInstaller($@"{PathLocator.Executable.NSudo} -U:T -P:E -ShowWindowMode:Hide -Wait cmd /c reg add HKLM\SYSTEM\CurrentControlSet\Services\{service.Key} /v Start /t REG_DWORD /d {service.Value} /f");
            }

            SetTaskState(true, winDefenderTasks);
            InvokePowerShell(@"Get-AppXpackage Microsoft.WindowsDefender | Foreach {Add-AppxPackage -DisableDevelopmentMode -Register ""$($_.InstallLocation)\AppXManifest.xml""}");
        }

        private static void Deactivate()
        {
            ExportRights();

            TerminateProcess();

            foreach (var (path, normal, block) in fileMappings)
            {
                CommandExecutor.RunCommandAsTrustedInstaller($"{PathLocator.Executable.NSudo} -U:T -P:E -ShowWindowMode:Hide -Wait cmd /c rename \"{Path.Combine(path, normal)}\" \"{block}\"");
            }

            foreach (var service in services)
            {
                CommandExecutor.RunCommandAsTrustedInstaller($@"{PathLocator.Executable.NSudo} -U:T -P:E -ShowWindowMode:Hide -Wait cmd /c reg add HKLM\SYSTEM\CurrentControlSet\Services\{service.Key} /v Start /t REG_DWORD /d 4 /f");
            }

            CommandExecutor.RunCommandAsTrustedInstaller($"{PathLocator.Executable.NSudo} -U:T -P:E -ShowWindowMode:Hide -Wait cmd /c " +
            $"for /d %D in (\"{Path.Combine(PathLocator.Folders.SystemDrive, @"ProgramData\\Microsoft\\Windows Defender\\Platform\\*")}\") do if exist \"%D\\MsMpEng.exe\" ren \"%D\\MsMpEng.exe\" BlockAntimalware.exe & " +
            $"for /d %D in (\"{Path.Combine(PathLocator.Folders.SystemDrive, @"ProgramData\\Microsoft\\Windows Defender\\Platform\\*")}\") do if exist \"%D\\MpDefenderCoreService.exe\" ren \"%D\\MpDefenderCoreService.exe\" BlockAntimalwareCore.exe & " +
            @"reg add HKLM\SYSTEM\CurrentControlSet\Services\WinDefend /v AutorunsDisabled /t REG_DWORD /d 3 /f & " +
            @"reg add HKLM\SYSTEM\CurrentControlSet\Services\wscsvc /v DelayedAutoStart /t REG_DWORD /d 1 /f & " +
            @"reg add ""HKLM\SOFTWARE\Microsoft\Windows Defender\Windows Defender\ControlledFolderAccess"" /v Enable /t REG_DWORD /d 0 /f & " +
            @"reg add ""HKLM\SOFTWARE\Microsoft\Windows defender\Windows defender Exploit Guard\ASR"" /v EnableASRConsumers /t REG_DWORD /d 0 /f & " +
            @"reg add ""HKLM\SOFTWARE\Microsoft\Windows defender\Windows defender Exploit Guard\Controlled Folder Access"" /v EnableControlledFolderAccess /t REG_DWORD /d 0 /f & " +
            @"reg add ""HKLM\SOFTWARE\Microsoft\Windows defender\Windows defender Exploit Guard\Network Protection"" /v EnableNetworkProtection /t REG_DWORD /d 0 /f & " +
            @"reg add ""HKLM\SOFTWARE\Microsoft\Windows defender\CoreService"" /v DisableCoreService1DSTelemetry /t REG_DWORD /d 1 /f & " +
            @"reg add ""HKLM\SOFTWARE\Microsoft\Windows defender\CoreService"" /v DisableCoreServiceECSIntegration /t REG_DWORD /d 1 /f & " +
            @"reg add ""HKLM\SOFTWARE\Microsoft\Windows defender\CoreService"" /v MdDisableResController /t REG_DWORD /d 1 /f & " +
            @"reg delete ""HKLM\SYSTEM\CurrentControlSet\Services\WdFilter\Instances\WdFilter Instance"" /v Altitude /f & " +
            @"reg add ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer"" /v SmartScreenEnabled /t REG_SZ /d off /f & " +
            @"reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\System"" /v EnableSmartScreen /t REG_DWORD /d 0 /f & " +
            @"reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender"" /v DisableAntiSpyware /t REG_DWORD /d 1 /f & " +
            @"reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender"" /v DisableAntiVirus /t REG_DWORD /d 1 /f");

            InvokePowerShell(@"Set-MpPreference -DisableIOAVProtection $true
            Set-MpPreference -DisableRealtimeMonitoring $true
            Set-MpPreference -DisableBehaviorMonitoring $true
            Set-MpPreference -DisableBlockAtFirstSeen $true
            Set-MpPreference -DisablePrivacyMode $true
            Set-MpPreference -SignatureDisableUpdateOnStartupWithoutEngine $true
            Set-MpPreference -DisableArchiveScanning $true
            Set-MpPreference -DisableIntrusionPreventionSystem $true
            Set-MpPreference -DisableScriptScanning $true
            Set-MpPreference -SubmitSamplesConsent 2
            Set-MpPreference -MAPSReporting 0
            Set-MpPreference -PUAProtection Disabled");

            CommandExecutor.RunCommandAsTrustedInstaller($"{PathLocator.Executable.NSudo} -U:T -P:E -ShowWindowMode:Hide -Wait cmd /c " +
            @"reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"" /v LocalSettingOverrideDisableOnAccessProtection /t REG_DWORD /d 0 /f & " +
            @"reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"" /v LocalSettingOverrideRealtimeScanDirection /t REG_DWORD /d 0 /f & " +
            @"reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"" /v LocalSettingOverrideDisableIOAVProtection /t REG_DWORD /d 0 /f & " +
            @"reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"" /v LocalSettingOverrideDisableBehaviorMonitoring /t REG_DWORD /d 0 /f & " +
            @"reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"" /v LocalSettingOverrideDisableIntrusionPreventionSystem /t REG_DWORD /d 0 /f & " +
            @"reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"" /v LocalSettingOverrideDisableRealtimeMonitoring /t REG_DWORD /d 0 /f & " +
            @"reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"" /v DisableBehaviorMonitoring /t REG_DWORD /d 1 /f & " +
            @"reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"" /v DisableIOAVProtection /t REG_DWORD /d 1 /f & " +
            @"reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"" /v DisableOnAccessProtection /t REG_DWORD /d 1 /f & " +
            @"reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"" /v DisableRealtimeMonitoring /t REG_DWORD /d 1 /f & " +
            @"reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"" /v DisableRoutinelyTakingAction /t REG_DWORD /d 1 /f & " +
            @"reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"" /v DisableScanOnRealtimeEnable /t REG_DWORD /d 1 /f & " +
            @"reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"" /v DisableScriptScanning /t REG_DWORD /d 1 /f & " +
            @"reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"" /v DisableRawWriteNotification /t REG_DWORD /d 1 /f & " +
            @"reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"" /v DisableIntrusionPreventionSystem /t REG_DWORD /d 1 /f & " +
            @"reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"" /v DisableArchiveScanning /t REG_DWORD /d 1 /f & " +
            @"reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"" /v SignatureDisableUpdateOnStartupWithoutEngine /t REG_DWORD /d 1 /f & " +
            @"reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"" /v DisablePrivacyMode /t REG_DWORD /d 1 /f & " +
            @"reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"" /v DisableBlockAtFirstSeen /t REG_DWORD /d 1 /f");

            SetTaskState(false, winDefenderTasks);

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

            CommandExecutor.RunCommandAsTrustedInstaller($"{PathLocator.Executable.NSudo} -U:T -P:E -ShowWindowMode:Hide -Wait cmd /c " +
            @"reg add ""HKLM\Software\Policies\Microsoft\Windows Defender\SpyNet"" /v DisableBlockAtFirstSeen /t REG_DWORD /d 1 /f & " +
            @"reg add ""HKLM\Software\Policies\Microsoft\Windows Defender\SpyNet"" /v SpynetReporting /t REG_DWORD /d 0 /f & " +
            @"reg add ""HKLM\Software\Policies\Microsoft\Windows Defender\SpyNet"" /v SubmitSamplesConsent /t REG_DWORD /d 2 /f & " +
            @"reg add ""HKLM\Software\Policies\Microsoft\Windows Defender\SpyNet"" /v LocalSettingOverrideSpynetReporting /t REG_DWORD /d 0 /f");

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

            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run", "SecurityHealth", new byte[] { 0099, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99 }, RegistryValueKind.Binary);
            RegistryHelp.DeleteValue(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "Windows Defender");
            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "SecurityHealth");

            CommandExecutor.RunCommandAsTrustedInstaller($"{PathLocator.Executable.NSudo} -U:T -P:E -ShowWindowMode:Hide -Wait cmd /c " +
            @"reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\AppHost"" /v EnableWebContentEvaluation /t REG_DWORD /d 0 /f & " +
            @"reg add ""HKLM\SOFTWARE\Policies\Microsoft\MicrosoftEdge\PhishingFilter"" /v EnabledV9 /t REG_DWORD /d 0 /f & " +
            @"reg add ""HKLM\SOFTWARE\Microsoft\Windows Defender"" /v PUAProtection /t REG_DWORD /d 0 /f & " +
            @"reg add ""HKLM\SOFTWARE\Microsoft\Windows Defender\Features"" /v TamperProtection /t REG_DWORD /d 0 /f & " +
            @"reg add ""HKLM\SOFTWARE\Policies\Microsoft\MRT"" /v DontOfferThroughWUA /t REG_DWORD /d 1 /f & " +
            @"reg add ""HKLM\SOFTWARE\Policies\Microsoft\MRT"" /v DontReportInfectionInformation /t REG_DWORD /d 1 /f & " +
            @"reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\SmartScreen"" /v ConfigureAppInstallControl /t REG_SZ /d ""Anywhere"" /f & " +
            @"reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\SmartScreen"" /v DontReportInfectionInformation /t REG_DWORD /d 0 /f & " +
            @"reg add ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\WINEVT\Channels\Microsoft-Windows-DeviceConfidence/Analytic"" /v Enabled /t REG_DWORD /d 0 /f & " +
            @"reg add ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\WINEVT\Channels\Microsoft-Windows-DeviceGuard/Operational"" /v Enabled /t REG_DWORD /d 0 /f & " +
            @"reg add ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\WINEVT\Channels\Microsoft-Windows-DeviceGuard/Verbose"" /v Enabled /t REG_DWORD /d 0 /f");

            RegistryHelp.DeleteFolderTree(Registry.ClassesRoot, @"*\shellex\ContextMenuHandlers\EPP");
            RegistryHelp.DeleteFolderTree(Registry.ClassesRoot, @"Directory\shellex\ContextMenuHandlers\EPP");
            RegistryHelp.DeleteFolderTree(Registry.ClassesRoot, @"Drive\shellex\ContextMenuHandlers\EPP");
            RegistryHelp.DeleteFolderTree(Registry.ClassesRoot, @"CLSID\{09A47860-11B0-4DA5-AFA5-26D86198A780}\InprocServer32");

            CommandExecutor.RunCommand("Get-PSDrive -PSProvider FileSystem | ForEach-Object { Add-MpPreference -ExclusionPath \"$($_.Root)\" } ", true);
            ManageExclusions(true);

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
                        CommandExecutor.RunCommandAsTrustedInstaller($"{PathLocator.Executable.NSudo} -U:T -P:E -ShowWindowMode:Hide -Wait cmd /c takeown /f \"{filePath}\" & icacls \"{filePath}\" /inheritance:r /remove S-1-5-32-544 S-1-5-11 S-1-5-32-545 S-1-5-18 & icacls \"{filePath}\" /grant {Environment.UserName}:F & rd /s /q \"{filePath}\"");

                        for (int i = 0; Directory.Exists(filePath) && i < 10; i++)
                        {
                            try { Directory.Delete(filePath, true); }
                            catch (Exception ex) { ErrorLogging.LogDebug(ex); }

                            TakingOwnership.GrantAdministratorsAccess(filePath, TakingOwnership.SE_OBJECT_TYPE.SE_FILE_OBJECT);
                            CommandExecutor.RunCommandAsTrustedInstaller($"{PathLocator.Executable.NSudo} -U:T -P:E -ShowWindowMode:Hide -Wait cmd /c del /q \"{filePath}\"");

                            Thread.Sleep(500);
                        }
                    }
                }
                catch (Exception ex) { ErrorLogging.LogDebug(ex); }
            }

            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "ConsentPromptBehaviorAdmin", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableInstallerDetection", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableLUA", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableSecureUIAPaths", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableVirtualization", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "FilterAdministratorToken", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "PromptOnSecureDesktop", 0, RegistryValueKind.DWord);
            RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Run", Path.GetFileNameWithoutExtension(PathLocator.Executable.DisablingWD), $@"""{PathLocator.Executable.DisablingWD}""", RegistryValueKind.String);
        }

        private static void ManageExclusions(bool isDisable)
        {
            foreach (string drive in DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed || d.DriveType == DriveType.Removable).Select(d => d.Name))
            {
                CommandExecutor.RunCommand($" {(isDisable ? "Add-MpPreference" : "Remove-MpPreference")} -ExclusionPath '{drive}'", true);
            }
        }

        private static void TerminateProcess()
        {
            string[] processes = { "smartscreen", "mpdefendercoreservice", "msmpeng", "securityhealthservice", "securityhealthsystray", "securityhealthui", "wuauserv", "searchui", "sechealthui", "configsecuritypolicy", "runtimebroker", "msedge", "ssoncom", "usocoreworker", "defenderbootstrapper", "configsecuritypolicy", "dlpuseragent", "lsass", "mpam-d", "mpam-fe", "mpam-fe_bd", "mpas-d", "mpas-fe", "mpas-fe_bd", "mpav-d", "mpav-fe", "mpav-fe_bd", "mpcmdrun", "mpcopyaccelerator", "mpdlpcmd", "mpdlpservice", "mpextms", "mpsigstub", "mrt", "msmpengcp", "mssense", "nissrv", "offlinescannershell", "securekernel", "securityhealthhost", "senseap", "senseaptoast", "sensecm", "sensegpparser", "senseidentity", "senseimdscollector", "senseir", "sensendr", "sensesampleuploader", "sensetvm", "sensece", "sgrmbroker", "healthattestationclientagent" };
            CommandExecutor.RunCommandAsTrustedInstaller($"/c taskkill /f " + string.Join(" ", processes.Select(p => $"/im {p}.exe")));
        }

        private static void InvokePowerShell(string command)
        {
            try
            {
                Process comandoAEjecutar = new Process();
                comandoAEjecutar.StartInfo.FileName = PathLocator.Executable.PowerShell;
                comandoAEjecutar.StartInfo.Arguments =
                    "-NoLogo -NonInteractive -NoProfile -ExecutionPolicy Bypass -EncodedCommand \"" +
                    Convert.ToBase64String(Encoding.Unicode.GetBytes(command)) + "\"";
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

