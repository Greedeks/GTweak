using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Managers;
using Microsoft.Win32;

namespace GTweak.Utilities.Tweaks
{
    internal sealed class ServicesTweaks : FirewallManager
    {
        internal readonly static Dictionary<string, object> ControlStates = new Dictionary<string, object>();
        private readonly ControlWriterManager _сontrolWriter = new ControlWriterManager(ControlStates);

        internal void AnalyzeAndUpdate()
        {
            _сontrolWriter.Button[1] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WSearch", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\fhsvc", "Start", "4");

            _сontrolWriter.Button[2] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\XboxGipSvc", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\XblAuthManager", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\XboxNetApiSvc", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\XblGameSave", "Start", "4");

            _сontrolWriter.Button[3] = PathLocator.Targets.WindowsUpdate.Mappings.All(f => File.Exists(f.Normal));

            _сontrolWriter.Button[4] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WalletService", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\VacSvc", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\spectrum", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\SharedRealitySvc", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\perceptionsimulation", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\MixedRealityOpenXRSvc", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\MapsBroker", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\EntAppSvc", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\embeddedmode", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\wlidsvc", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WEPHOSTSVC", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\StorSvc", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\ClipSVC", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\InstallService", "Start", "4");

            _сontrolWriter.Button[5] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\wmiApSrv", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\pla", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\PerfHost", "Start", "4");

            _сontrolWriter.Button[6] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WbioSrvc", "Start", "4");

            _сontrolWriter.Button[7] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\bthserv", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\BthAvctpSvc", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\BTAGService", "Start", "4");

            _сontrolWriter.Button[8] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Spooler", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\PrintNotify", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\McpManagementService", "Start", "4") || IsTaskEnabled(printTasks);

            _сontrolWriter.Button[9] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WiaRpc", "Start", "4");

            _сontrolWriter.Button[10] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\TapiSrv", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\PhoneSvc", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Fax", "Start", "4");

            _сontrolWriter.Button[11] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\SensrSvc", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\SensorService", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\SensorDataService", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\SEMgrSvc", "Start", "4");

            _сontrolWriter.Button[12] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WFDSConMgrSvc", "Start", "4");

            _сontrolWriter.Button[13] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\CDPSvc", "Start", "4");

            _сontrolWriter.Button[14] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Netlogon", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\CscService", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\lmhosts", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\FDResPub", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\fdPHost", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\LanmanServer", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\LanmanWorkstation", "Start", "4");

            _сontrolWriter.Button[15] =
                 RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WwanSvc", "Start", "4") ||
                 RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\wlpasvc", "Start", "4") ||
                 RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\icssvc", "Start", "4") ||
                 RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\DusmSvc", "Start", "4") ||
                 RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\autotimesvc", "Start", "4");

            _сontrolWriter.Button[16] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\PolicyAgent", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\IKEEXT", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\p2pimsvc", "Start", "4");

            _сontrolWriter.Button[17] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WPDBusEnum", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WMPNetworkSvc", "Start", "4");

            _сontrolWriter.Button[18] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\UmRdpService", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\TermService", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\SessionEnv", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\DsSvc", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\RemoteRegistry", "Start", "4");

            _сontrolWriter.Button[19] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WerSvc", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\wercplsupport", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Wecsvc", "Start", "4");

            _сontrolWriter.Button[20] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WebClient", "Start", "4");

            _сontrolWriter.Button[21] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\SCPolicySvc", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\ScDeviceEnum", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\SCardSvr", "Start", "4");

            _сontrolWriter.Button[22] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\AssignedAccessManagerSvc", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\AppReadiness", "Start", "4");

            _сontrolWriter.Button[23] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\BDESVC", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\EFS", "Start", "4");

            _сontrolWriter.Button[24] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\LxpSvc", "Start", "4");

            _сontrolWriter.Button[25] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WarpJITSvc", "Start", "4");

            _сontrolWriter.Button[26] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WdiSystemHost", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WdiServiceHost", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\TroubleshootingSvc", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\DPS", "Start", "4");

            _сontrolWriter.Button[27] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\workfolderssvc", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\dot3svc", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\DevQueryBroker", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\AppMgmt", "Start", "4");

            _сontrolWriter.Button[28] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\vmicvmsession", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\vmictimesync", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\vmicshutdown", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\vmicrdv", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\vmickvpexchange", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\vmicheartbeat", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\vmicguestinterface", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\HvHost", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\vmicvss", "Start", "4");

            _сontrolWriter.Button[29] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\PushToInstall", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WpnService", "Start", "4");

            _сontrolWriter.Button[30] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\RetailDemo", "Start", "4");

            _сontrolWriter.Button[31] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\edgeupdate", "Start", "4") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\edgeupdatem", "Start", "4") ||
                RegistryHelp.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\WindowsUpdate\Orchestrator\UScheduler_Oobe\EdgeUpdate", "cmdLine", string.Empty).IndexOf(PathLocator.Targets.WindowsUpdate.UsoClient.Normal, StringComparison.OrdinalIgnoreCase) >= 0 ||
                IsTaskEnabled(edgeTasks);

            _сontrolWriter.Button[32] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\MMCSS", "Start", "4");

            _сontrolWriter.Button[33] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\lfsvc", "Start", "4") || IsTaskEnabled(geoTasks);
        }

        internal void ApplyTweaks(string tweak, bool state)
        {
            INIManager.TempWrite(INIManager.TempTweaksSvc, tweak, state);

            switch (tweak)
            {
                case "TglButton1":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WSearch", "Start", state ? 2 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\fhsvc", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    break;
                case "TglButton2":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\XboxGipSvc", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\XblAuthManager", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\XboxNetApiSvc", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\XblGameSave", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    SetTaskState(state, xboxTasks);
                    break;
                case "TglButton3":
                    BlockWindowsUpdate(state);
                    ChangeAccessUpdateFolders(state);

                    foreach ((string Normal, string Block) in PathLocator.Targets.WindowsUpdate.Mappings)
                    {
                        string currentFilePath = state ? Block : Normal;
                        string targetFilePath = state ? Normal : Block;

                        try
                        {
                            if (state)
                            {
                                CommandExecutor.RunCommandAsTrustedInstaller($@"/c ""{CommandExecutor.CleanCommand(string.Join(" && ", new[] {
                                $@"(if exist ""{targetFilePath}"" (takeown /f ""{targetFilePath}"" /a && del /f /q /a ""{targetFilePath}"") else (cd .))",
                                $@"takeown /f ""{currentFilePath}"" /a",
                                $@"rename ""{currentFilePath}"" ""{Path.GetFileName(targetFilePath)}""",
                                $@"icacls ""{targetFilePath}"" /setowner *S-1-5-80-956008885-3418522649-1831038044-1853292631-2271478464",
                                $@"icacls ""{targetFilePath}"" /inheritance:r",
                                $@"icacls ""{targetFilePath}"" /grant *S-1-5-32-544:F",
                                $@"icacls ""{targetFilePath}"" /grant *S-1-5-32-545:R",
                                $@"icacls ""{targetFilePath}"" /grant *S-1-5-18:F",
                                $@"icacls ""{targetFilePath}"" /grant *S-1-5-80-956008885-3418522649-1831038044-1853292631-2271478464:F",
                                $@"icacls ""{targetFilePath}"" /grant *S-1-15-2-1:R",
                                $@"icacls ""{targetFilePath}"" /grant *S-1-15-2-2:R",
                                $@"icacls ""{targetFilePath}"" /remove ""{Environment.UserName}"""}))}""");
                            }
                            else
                            {
                                CommandExecutor.RunCommandAsTrustedInstaller($@"/c ""{CommandExecutor.CleanCommand(string.Join(" && ", new[] {
                                $@"takeown /f ""{currentFilePath}"" /a",
                                $@"icacls ""{currentFilePath}"" /inheritance:r /remove *S-1-5-32-544 *S-1-5-11 *S-1-5-32-545 *S-1-5-18",
                                $@"icacls ""{currentFilePath}"" /grant ""{Environment.UserName}"":F",
                                $@"(if exist ""{targetFilePath}"" (takeown /f ""{targetFilePath}"" /a && del /f /q /a ""{targetFilePath}"") else (cd .))",
                                $@"rename ""{currentFilePath}"" ""{Path.GetFileName(targetFilePath)}"""}))}""");
                            }
                        }
                        catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                    }

                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\wisvc", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\DmEnrollmentSvc", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\wuauserv", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WaaSMedicSvc", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\DoSvc", "Start", state ? 2 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\UsoSvc", "Start", state ? 2 : 4, RegistryValueKind.DWord);

                    if (state)
                    {
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\WindowsUpdate");
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate");
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\WindowsUpdate\AU", "AutomaticMaintenanceEnabled", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", "DisableWindowsUpdateAccess", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", "DoNotConnectToWindowsUpdateInternetLocations", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "NoAutoUpdate", 1, RegistryValueKind.DWord);
                    }
                    break;
                case "TglButton4":
                    CommandExecutor.RunCommandAsTrustedInstaller($@"/c {string.Join(" & ", new[] { "WalletService", "VacSvc", "spectrum", "SharedRealitySvc", "perceptionsimulation", "MixedRealityOpenXRSvc", "MapsBroker", "EntAppSvc",
                        "embeddedmode", "wlidsvc", "WEPHOSTSVC", "StorSvc", "ClipSVC", "InstallService" }.Select(s => $@"reg add HKLM\SYSTEM\CurrentControlSet\Services\{s} /v Start /t REG_DWORD /d {(state ? "3" : "4")} /f"))}");
                    break;
                case "TglButton5":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\wmiApSrv", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\pla", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\PerfHost", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    break;
                case "TglButton6":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WbioSrvc", "Start", state ? 2 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WbioSrvc", "DelayedAutoStart", state ? 0 : 1, RegistryValueKind.DWord);
                    break;
                case "TglButton7":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\bthserv", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\BthAvctpSvc", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\BTAGService", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    SetTaskState(state, bluetoothTask);
                    break;
                case "TglButton8":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\Spooler", "Start", state ? 2 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\Spooler", "DelayedAutoStart", state ? 0 : 1, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\PrintNotify", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\McpManagementService", "Start", state ? 3 : 4, RegistryValueKind.DWord);

                    if (!state)
                    {
                        CommandExecutor.RunCommandAsTrustedInstaller(@$"/c net stop spooler && del /F /Q %systemroot%\System32\spool\PRINTERS\*.*");
                    }

                    try
                    {
                        using RegistryKey baseKey = Registry.ClassesRoot.OpenSubKey("SystemFileAssociations", true);
                        if (baseKey != null)
                        {
                            foreach (string subkey in baseKey.GetSubKeyNames())
                            {
                                try
                                {
                                    using RegistryKey assocKey = baseKey.OpenSubKey(subkey, true);
                                    if (assocKey != null)
                                    {
                                        using RegistryKey shellKey = assocKey.OpenSubKey("Shell", true);
                                        if (shellKey != null)
                                        {
                                            if (shellKey.GetSubKeyNames().Any(k => k.Equals("print", StringComparison.OrdinalIgnoreCase)))
                                            {
                                                if (state)
                                                {
                                                    RegistryHelp.DeleteValue(Registry.ClassesRoot, $@"SystemFileAssociations\{subkey}\shell\print", "ProgrammaticAccessOnly");
                                                }
                                                else
                                                {
                                                    RegistryHelp.Write(Registry.ClassesRoot, $@"SystemFileAssociations\{subkey}\shell\print", "ProgrammaticAccessOnly", string.Empty, RegistryValueKind.String);
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                            }
                        }
                    }
                    catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                    SetTaskState(state, printTasks);
                    break;
                case "TglButton9":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WiaRpc", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    break;
                case "TglButton10":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\TapiSrv", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\PhoneSvc", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\Fax", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    break;
                case "TglButton11":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SensrSvc", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SensorService", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SensorDataService", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SEMgrSvc", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    break;
                case "TglButton12":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WFDSConMgrSvc", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    break;
                case "TglButton13":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\CDPSvc", "Start", state ? 2 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\CDPSvc", "DelayedAutoStart", state ? 0 : 1, RegistryValueKind.DWord);
                    break;
                case "TglButton14":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\Netlogon", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\CscService", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\lmhosts", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\FDResPub", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\fdPHost", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\LanmanServer", "Start", state ? 2 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\LanmanWorkstation", "Start", state ? 2 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\LanmanServer", "DelayedAutoStart", state ? 0 : 1, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\LanmanWorkstation", "DelayedAutoStart", state ? 0 : 1, RegistryValueKind.DWord);
                    break;
                case "TglButton15":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WwanSvc", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\wlpasvc", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\icssvc", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\DusmSvc", "Start", state ? 2 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\autotimesvc", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\DusmSvc", "DelayedAutoStart", state ? 0 : 1, RegistryValueKind.DWord);
                    break;
                case "TglButton16":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\PolicyAgent", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\IKEEXT", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\p2pimsvc", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    break;
                case "TglButton17":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WPDBusEnum", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WMPNetworkSvc", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    break;
                case "TglButton18":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\UmRdpService", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\TermService", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SessionEnv", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\DsSvc", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\RemoteRegistry", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    break;
                case "TglButton19":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WerSvc", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\wercplsupport", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\Wecsvc", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    break;
                case "TglButton20":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WebClient", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    break;
                case "TglButton21":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SCPolicySvc", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\ScDeviceEnum", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SCardSvr", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\CertPropSvc", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    break;
                case "TglButton22":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\AssignedAccessManagerSvc", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\AppReadiness", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    break;
                case "TglButton23":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\BDESVC", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\EFS", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    break;
                case "TglButton24":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\LxpSvc", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    break;
                case "TglButton25":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WarpJITSvc", "Start", state ? 3 : 4, RegistryValueKind.DWord, true);
                    break;
                case "TglButton26":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WdiServiceHost", "Start", state ? 3 : 4, RegistryValueKind.DWord, true);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WdiSystemHost", "Start", state ? 3 : 4, RegistryValueKind.DWord, true);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\TroubleshootingSvc", "Start", state ? 3 : 4, RegistryValueKind.DWord, true);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\DPS", "Start", state ? 2 : 4, RegistryValueKind.DWord, true);
                    break;
                case "TglButton27":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\workfolderssvc", "Start", state ? 3 : 4, RegistryValueKind.DWord, true);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\dot3svc", "Start", state ? 3 : 4, RegistryValueKind.DWord, true);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\DevQueryBroker", "Start", state ? 3 : 4, RegistryValueKind.DWord, true);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\AppMgmt", "Start", state ? 3 : 4, RegistryValueKind.DWord, true);
                    break;
                case "TglButton28":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\vmicvmsession", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\vmictimesync", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\vmicshutdown", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\vmicrdv", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\vmickvpexchange", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\vmicheartbeat", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\vmicguestinterface", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\HvHost", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\vmicvss", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    break;
                case "TglButton29":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\PushToInstall", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WpnService", "Start", state ? 2 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WpnService", "DelayedAutoStart", state ? 0 : 1, RegistryValueKind.DWord);
                    break;
                case "TglButton30":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\RetailDemo", "Start", state ? 3 : 4, RegistryValueKind.DWord, true);
                    SetTaskState(state, retailTasks);
                    break;
                case "TglButton31":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\edgeupdate", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\edgeupdatem", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\WindowsUpdate\Orchestrator\UScheduler_Oobe\EdgeUpdate", "cmdLine", state ? PathLocator.Targets.WindowsUpdate.UsoClient.Normal : "dllhost.exe", RegistryValueKind.String);
                    SetTaskState(state, edgeTasks);
                    break;
                case "TglButton32":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\MMCSS", "Start", state ? 3 : 4, RegistryValueKind.DWord, true);
                    break;
                case "TglButton33":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\lfsvc", "Start", state ? 3 : 4, RegistryValueKind.DWord);
                    SetTaskState(state, geoTasks);
                    break;
                default:
                    break;
            }
        }

        private static void ChangeAccessUpdateFolders(bool state)
        {
            Task.Run(delegate
            {
                try
                {
                    if (state)
                    {
                        SetTaskStateOwner(true, winUpdatesTasks);
                    }
                    else
                    {
                        CommandExecutor.RunCommandAsTrustedInstaller($"/c net stop wuauserv & net stop bits & net stop cryptSvc & net stop RuntimeBroker & taskkill /f " + string.Join(" ", (new string[] { "usocoreworker", "msedge", "pwahelper", "edgeupdate", "edgeupdatem", "microsoftedgeupdate", "msedgewebviewhost", "msedgeuserbroker", "runtimebroker", "widgets" }).Select(p => $"/im {p}.exe")));

                        foreach (string path in PathLocator.Targets.WindowsUpdate.CleanupFolders)
                        {
                            UnlockHandleHelper.UnlockDirectory(path);

                            CommandExecutor.RunCommandAsTrustedInstaller($@"/c takeown /f ""{path}"" /r /d y && icacls ""{path}"" /inheritance:r && icacls ""{path}"" /remove *S-1-5-32-544 *S-1-5-11 *S-1-5-32-545 *S-1-5-18 && icacls ""{path}"" /grant {Environment.UserName}:F /t && rd /s /q ""{path}""");

                            for (int i = 0; Directory.Exists(path) && i < 5; i++)
                            {
                                try { Directory.Delete(path, true); }
                                catch (Exception ex) { ErrorLogging.LogDebug(ex); }

                                CommandExecutor.RunCommand($"Remove-Item -LiteralPath '{path}' -Recurse -Force", true);
                            }
                        }

                        SetTaskStateOwner(false, winUpdatesTasks);
                        CommandExecutor.RunCommandAsTrustedInstaller("/c net start bits & net start cryptSvc");
                    }
                }
                catch (Exception ex) { ErrorLogging.LogDebug(ex); }
            });
        }
    }
}
