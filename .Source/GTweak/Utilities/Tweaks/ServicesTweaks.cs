using GTweak.Utilities.Helpers;
using GTweak.View;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace GTweak.Utilities.Tweaks
{
    internal sealed class ServicesTweaks : Firewall
    {
        private static readonly string filesPathUpdate = Settings.PathSystemDisk + "Windows\\System32\\Tasks\\Microsoft\\Windows";

        internal void ViewServices(ServicesView servicesV)
        {
            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WSearch", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WSearch", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\fhsvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\fhsvc", "Start", string.Empty).ToString() != "4")
                servicesV.TglButton1.StateNA = true;
            else
                servicesV.TglButton1.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\XboxGipSvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\XboxGipSvc", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\XblAuthManager", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\XblAuthManager", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\XboxNetApiSvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\XboxNetApiSvc", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\XblGameSave", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\XblGameSave", "Start", string.Empty).ToString() != "4")
                servicesV.TglButton2.StateNA = true;
            else
                servicesV.TglButton2.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WwanSvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WwanSvc", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\wlpasvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\wlpasvc", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\icssvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\icssvc", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\DusmSvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\DusmSvc", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\autotimesvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\autotimesvc", "Start", string.Empty).ToString() != "4")
                servicesV.TglButton3.StateNA = true;
            else
                servicesV.TglButton3.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WalletService", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WalletService", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\VacSvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\VacSvc", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\spectrum", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\spectrum", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\SharedRealitySvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\SharedRealitySvc", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\perceptionsimulation", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\perceptionsimulation", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\MixedRealityOpenXRSvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\MixedRealityOpenXRSvc", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\MapsBroker", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\MapsBroker", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\EntAppSvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\EntAppSvc", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\embeddedmode", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\embeddedmode", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\wlidsvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\wlidsvc", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WEPHOSTSVC", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WEPHOSTSVC", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\StorSvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\StorSvc", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\ClipSVC", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\ClipSVC", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\InstallService", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\InstallService", "Start", string.Empty).ToString() != "4")
                servicesV.TglButton4.StateNA = true;
            else
                servicesV.TglButton4.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\wmiApSrv", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\wmiApSrv", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\pla", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\pla", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\PerfHost", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\PerfHost", "Start", string.Empty).ToString() != "4")
                servicesV.TglButton5.StateNA = true;
            else
                servicesV.TglButton5.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WbioSrvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WbioSrvc", "Start", string.Empty).ToString() != "4")
                servicesV.TglButton6.StateNA = true;
            else
                servicesV.TglButton6.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\bthserv", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\bthserv", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\BthAvctpSvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\BthAvctpSvc", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\BTAGService", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\BTAGService", "Start", string.Empty).ToString() != "4")
                servicesV.TglButton7.StateNA = true;
            else
                servicesV.TglButton7.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Spooler", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Spooler", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\PrintNotify", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\PrintNotify", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\McpManagementService", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\McpManagementService", "Start", string.Empty).ToString() != "4")
                servicesV.TglButton8.StateNA = true;
            else
                servicesV.TglButton8.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WiaRpc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WiaRpc", "Start", string.Empty).ToString() != "4")
                servicesV.TglButton9.StateNA = true;
            else
                servicesV.TglButton9.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\TapiSrv", "Start", null) == null ||
                 Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\TapiSrv", "Start", string.Empty).ToString() != "4" ||
                 Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\PhoneSvc", "Start", null) == null ||
                 Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\PhoneSvc", "Start", string.Empty).ToString() != "4" ||
                 Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Fax", "Start", null) == null ||
                 Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Fax", "Start", string.Empty).ToString() != "4")
                servicesV.TglButton10.StateNA = true;
            else
                servicesV.TglButton10.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\SensrSvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\SensrSvc", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\SensorService", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\SensorService", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\SensorDataService", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\SensorDataService", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\SEMgrSvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\SEMgrSvc", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\lfsvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\lfsvc", "Start", string.Empty).ToString() != "4")
                servicesV.TglButton11.StateNA = true;
            else
                servicesV.TglButton11.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\DispBrokerDesktopSvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\DispBrokerDesktopSvc", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WFDSConMgrSvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WFDSConMgrSvc", "Start", string.Empty).ToString() != "4")
                servicesV.TglButton12.StateNA = true;
            else
                servicesV.TglButton12.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\CDPSvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\CDPSvc", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\PushToInstall", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\PushToInstall", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WpnService", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WpnService", "Start", string.Empty).ToString() != "4")
                servicesV.TglButton13.StateNA = true;
            else
                servicesV.TglButton13.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Netlogon", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Netlogon", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\CscService", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\CscService", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\lmhosts", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\lmhosts", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\FDResPub", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\FDResPub", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\fdPHost", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\fdPHost", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\LanmanServer", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\LanmanServer", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\LanmanWorkstation", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\LanmanWorkstation", "Start", string.Empty).ToString() != "4")
                servicesV.TglButton14.StateNA = true;
            else
                servicesV.TglButton14.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\wisvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\wisvc", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\DmEnrollmentSvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\DmEnrollmentSvc", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\wuauserv", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WaaSMedicSvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WaaSMedicSvc", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\DoSvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\DoSvc", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\UsoSvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\UsoSvc", "Start", string.Empty).ToString() != "4")
                servicesV.TglButton15.StateNA = true;
            else
                servicesV.TglButton15.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\PolicyAgent", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\PolicyAgent", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\IKEEXT", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\IKEEXT", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\p2pimsvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\p2pimsvc", "Start", string.Empty).ToString() != "4")
                servicesV.TglButton16.StateNA = true;
            else
                servicesV.TglButton16.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WPDBusEnum", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WPDBusEnum", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WMPNetworkSvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WMPNetworkSvc", "Start", string.Empty).ToString() != "4")
                servicesV.TglButton17.StateNA = true;
            else
                servicesV.TglButton17.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\UmRdpService", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\UmRdpService", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\TermService", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\TermService", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\SessionEnv", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\SessionEnv", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\DsSvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\DsSvc", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\RemoteRegistry", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\RemoteRegistry", "Start", string.Empty).ToString() != "4")
                servicesV.TglButton18.StateNA = true;
            else
                servicesV.TglButton18.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WerSvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WerSvc", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\wercplsupport", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\wercplsupport", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Wecsvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Wecsvc", "Start", string.Empty).ToString() != "4")
                servicesV.TglButton19.StateNA = true;
            else
                servicesV.TglButton19.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WebClient", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WebClient", "Start", string.Empty).ToString() != "4")
                servicesV.TglButton20.StateNA = true;
            else
                servicesV.TglButton20.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\SCPolicySvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\SCPolicySvc", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\ScDeviceEnum", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\ScDeviceEnum", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\SCardSvr", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\SCardSvr", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\SCPolicySvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\SCPolicySvc", "Start", string.Empty).ToString() != "4")
                servicesV.TglButton21.StateNA = true;
            else
                servicesV.TglButton21.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\AssignedAccessManagerSvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\AssignedAccessManagerSvc", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\AppReadiness", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\AppReadiness", "Start", string.Empty).ToString() != "4")
                servicesV.TglButton22.StateNA = true;
            else
                servicesV.TglButton22.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\BDESVC", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\BDESVC", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\EFS", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\EFS", "Start", string.Empty).ToString() != "4")
                servicesV.TglButton23.StateNA = true;
            else
                servicesV.TglButton23.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\LxpSvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\LxpSvc", "Start", string.Empty).ToString() != "4")
                servicesV.TglButton24.StateNA = true;
            else
                servicesV.TglButton24.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WarpJITSvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WarpJITSvc", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\wscsvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\wscsvc", "Start", string.Empty).ToString() != "4")
                servicesV.TglButton25.StateNA = true;
            else
                servicesV.TglButton25.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WdiSystemHost", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WdiSystemHost", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WdiServiceHost", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WdiServiceHost", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\TroubleshootingSvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\TroubleshootingSvc", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\DPS", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\DPS", "Start", string.Empty).ToString() != "4")
                servicesV.TglButton26.StateNA = true;
            else
                servicesV.TglButton26.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\workfolderssvc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\workfolderssvc", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\dot3svc", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\dot3svc", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\DevQueryBroker", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\DevQueryBroker", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\AppMgmt", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\AppMgmt", "Start", string.Empty).ToString() != "4")
                servicesV.TglButton27.StateNA = true;
            else
                servicesV.TglButton27.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\vmicvmsession", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\vmicvmsession", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\vmictimesync", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\vmictimesync", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\vmicshutdown", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\vmicshutdown", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\vmicrdv", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\vmicrdv", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\vmickvpexchange", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\vmickvpexchange", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\vmicheartbeat", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\vmicheartbeat", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\vmicguestinterface", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\vmicguestinterface", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\HvHost", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\HvHost", "Start", string.Empty).ToString() != "4" ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\vmicvss", "Start", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\vmicvss", "Start", string.Empty).ToString() != "4")
                servicesV.TglButton28.StateNA = true;
            else
                servicesV.TglButton28.StateNA = false;
        }

        internal static void UseServices(string tweak, bool isChoose)
        {
            switch (tweak)
            {
                case "TglButton1":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WSearch", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\fhsvc", "Start", 4, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WSearch", "Start", 2, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\fhsvc", "Start", 3, RegistryValueKind.DWord);
                    }
                    break;
                case "TglButton2":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\XboxGipSvc", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\XblAuthManager", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\XboxNetApiSvc", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\XblGameSave", "Start", 4, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\XboxGipSvc", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\XblAuthManager", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\XboxNetApiSvc", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\XblGameSave", "Start", 3, RegistryValueKind.DWord);
                    }
                    break;
                case "TglButton3":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WwanSvc", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\wlpasvc", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\icssvc", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\DusmSvc", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\autotimesvc", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\DusmSvc", "DelayedAutoStart", 1, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WwanSvc", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\wlpasvc", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\icssvc", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\DusmSvc", "Start", 2, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\autotimesvc", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\DusmSvc", "DelayedAutoStart", 0, RegistryValueKind.DWord);
                    }
                    break;
                case "TglButton4":
                    static void StoreServices(bool isChoose)
                    {
                        string value = isChoose ? "4" : "3";

                        TrustedInstaller.CreateProcessAsTrustedInstaller(Settings.PID, $"cmd.exe /c reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\WalletService /t REG_DWORD /v Start /d {value} /f & " +
                            $"reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\VacSvc /t REG_DWORD /v Start /d {value} /f & " +
                            $"reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\spectrum /t REG_DWORD /v Start /d {value} /f & " +
                            $"reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\SharedRealitySvc /t REG_DWORD /v Start /d {value} /f & " +
                            $"reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\perceptionsimulation /t REG_DWORD /v Start /d {value} /f & " +
                            $"reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\MixedRealityOpenXRSvc /t REG_DWORD /v Start /d {value} /f & " +
                            $"reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\MapsBroker /t REG_DWORD /v Start /d {value} /f & " +
                            $"reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\EntAppSvc /t REG_DWORD /v Start /d {value} /f & " +
                            $"reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\embeddedmode /t REG_DWORD /v Start /d {value} /f & " +
                            $"reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\wlidsvc /t REG_DWORD /v Start /d {value} /f & " +
                            $"reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\WEPHOSTSVC /t REG_DWORD /v Start /d {value} /f & " +
                            $"reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\StorSvc /t REG_DWORD /v Start /d {value} /f & " +
                            $"reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\ClipSVC /t REG_DWORD /v Start /d {value} /f & " +
                            $"reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\InstallService /t REG_DWORD /v Start /d {value} /f");
                    }

                    using (BackgroundWorker backgroundWorker = new BackgroundWorker())
                    {
                        backgroundWorker.DoWork += (s, e) => { StoreServices(isChoose); };
                        backgroundWorker.RunWorkerAsync();
                    }
                    break;
                case "TglButton5":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\wmiApSrv", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\pla", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\PerfHost", "Start", 4, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\wmiApSrv", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\pla", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\PerfHost", "Start", 3, RegistryValueKind.DWord);
                    }
                    break;
                case "TglButton6":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WbioSrvc", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WbioSrvc", "DelayedAutoStart", 1, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WbioSrvc", "Start", 2, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WbioSrvc", "DelayedAutoStart", 0, RegistryValueKind.DWord);
                    }
                    break;
                case "TglButton7":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\bthserv", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\BthAvctpSvc", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\BTAGService", "Start", 4, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\bthserv", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\BthAvctpSvc", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\BTAGService", "Start", 3, RegistryValueKind.DWord);
                    }
                    break;
                case "TglButton8":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\Spooler", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\PrintNotify", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\McpManagementService", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\Spooler", "DelayedAutoStart", 1, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\Spooler", "Start", 2, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\PrintNotify", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\McpManagementService", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\Spooler", "DelayedAutoStart", 0, RegistryValueKind.DWord);
                    }
                    break;
                case "TglButton9":
                    if (isChoose)
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WiaRpc", "Start", 4, RegistryValueKind.DWord);
                    else
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WiaRpc", "Start", 3, RegistryValueKind.DWord);
                    break;
                case "TglButton10":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\TapiSrv", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\PhoneSvc", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\Fax", "Start", 4, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\TapiSrv", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\PhoneSvc", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\Fax", "Start", 3, RegistryValueKind.DWord);
                    }
                    break;
                case "TglButton11":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SensrSvc", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SensorService", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SensorDataService", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SEMgrSvc", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\lfsvc", "Start", 4, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SensrSvc", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SensorService", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SensorDataService", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SEMgrSvc", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\lfsvc", "Start", 3, RegistryValueKind.DWord);
                    }
                    break;
                case "TglButton12":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\DispBrokerDesktopSvc", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WFDSConMgrSvc", "Start", 4, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\DispBrokerDesktopSvc", "Start", 2, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WFDSConMgrSvc", "Start", 3, RegistryValueKind.DWord);
                    }
                    break;
                case "TglButton13":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\CDPSvc", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\PushToInstall", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WpnService", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\CDPSvc", "DelayedAutoStart", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WpnService", "DelayedAutoStart", 1, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\CDPSvc", "Start", 2, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\PushToInstall", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WpnService", "Start", 2, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\CDPSvc", "DelayedAutoStart", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WpnService", "DelayedAutoStart", 0, RegistryValueKind.DWord);
                    }
                    break;
                case "TglButton14":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\Netlogon", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\CscService", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\lmhosts", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\FDResPub", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\fdPHost", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\LanmanServer", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\LanmanWorkstation", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\LanmanServer", "DelayedAutoStart", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\LanmanWorkstation", "DelayedAutoStart", 1, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\Netlogon", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\CscService", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\lmhosts", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\FDResPub", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\fdPHost", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\LanmanServer", "Start", 2, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\LanmanWorkstation", "Start", 2, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\LanmanServer", "DelayedAutoStart", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\LanmanWorkstation", "DelayedAutoStart", 0, RegistryValueKind.DWord);
                    }
                    break;
                case "TglButton15":
                    BlockWindowsUpdate(isChoose);
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\wisvc", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\DmEnrollmentSvc", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\wuauserv", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WaaSMedicSvc", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\DoSvc", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\UsoSvc", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\WindowsUpdate\AU", "AllowMUUpdateService", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\WindowsUpdate\AU", "AUOptions", 2, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\WindowsUpdate\AU", "AutomaticMaintenanceEnabled", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\WindowsUpdate\AU", "NoAutoUpdate", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\WindowsUpdate\AU", "NoAutoRebootWithLoggedOnUsers", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\WindowsUpdate\AU", "ScheduledInstallTime", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\WindowsUpdate\AU", "ScheduledInstallDay", 0, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\wisvc", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\DmEnrollmentSvc", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\wuauserv", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WaaSMedicSvc", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\DoSvc", "Start", 2, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\UsoSvc", "Start", 2, RegistryValueKind.DWord);
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\WindowsUpdate");
                    }
                    using (BackgroundWorker backgroundWorker = new BackgroundWorker())
                    {
                        backgroundWorker.DoWork += (s, e) => { ChangeAccessUpdateFolder(isChoose); };
                        backgroundWorker.RunWorkerAsync();
                    }
                    break;
                case "TglButton16":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\PolicyAgent", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\IKEEXT", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\p2pimsvc", "Start", 4, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\PolicyAgent", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\IKEEXT", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\p2pimsvc", "Start", 3, RegistryValueKind.DWord);
                    }
                    break;
                case "TglButton17":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WPDBusEnum", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WMPNetworkSvc", "Start", 4, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WPDBusEnum", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WMPNetworkSvc", "Start", 3, RegistryValueKind.DWord);
                    }
                    break;
                case "TglButton18":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\UmRdpService", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\TermService", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SessionEnv", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\DsSvc", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\RemoteRegistry", "Start", 4, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\UmRdpService", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\TermService", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SessionEnv", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\DsSvc", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\RemoteRegistry", "Start", 4, RegistryValueKind.DWord);
                    }
                    break;
                case "TglButton19":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WerSvc", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\wercplsupport", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\Wecsvc", "Start", 4, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WerSvc", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\wercplsupport", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\Wecsvc", "Start", 3, RegistryValueKind.DWord);
                    }
                    break;
                case "TglButton20":
                    if (isChoose)
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WebClient", "Start", 4, RegistryValueKind.DWord);
                    else
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WebClient", "Start", 3, RegistryValueKind.DWord);
                    break;
                case "TglButton21":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SCPolicySvc", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\ScDeviceEnum", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SCardSvr", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\CertPropSvc", "Start", 4, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SCPolicySvc", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\ScDeviceEnum", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SCardSvr", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\CertPropSvc", "Start", 3, RegistryValueKind.DWord);
                    }
                    break;
                case "TglButton22":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\AssignedAccessManagerSvc", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\AppReadiness", "Start", 4, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\AssignedAccessManagerSvc", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\AppReadiness", "Start", 3, RegistryValueKind.DWord);
                    }
                    break;
                case "TglButton23":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\BDESVC", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\EFS", "Start", 4, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\BDESVC", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\EFS", "Start", 3, RegistryValueKind.DWord);
                    }
                    break;
                case "TglButton24":
                    if (isChoose)
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\LxpSvc", "Start", 4, RegistryValueKind.DWord);
                    else
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\LxpSvc", "Start", 3, RegistryValueKind.DWord);
                    break;
                case "TglButton25":
                    if (isChoose)
                    {
                        TrustedInstaller.CreateProcessAsTrustedInstaller(Settings.PID, "cmd.exe /c reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\WarpJITSvc /t REG_DWORD /v Start /d 4 /f & " +
                            "reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\wscsvc /t REG_DWORD /v Start /d 4 /f & " +
                            "reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\wscsvc /t REG_DWORD /v DelayedAutoStart /d 1 /f");
                    }
                    else
                    {
                        TrustedInstaller.CreateProcessAsTrustedInstaller(Settings.PID, "cmd.exe /c reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\WarpJITSvc /t REG_DWORD /v Start /d 3 /f & " +
                            "reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\wscsvc /t REG_DWORD /v Start /d 2 /f & " +
                            "reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\wscsvc /t REG_DWORD /v DelayedAutoStart /d 0 /f");
                    }
                    break;
                case "TglButton26":
                    if (isChoose)
                    {
                        TrustedInstaller.CreateProcessAsTrustedInstaller(Settings.PID, "cmd.exe /c reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\WdiSystemHost /t REG_DWORD /v Start /d 4 /f & " +
                            "reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\WdiServiceHost /t REG_DWORD /v Start /d 4 /f & " +
                            "reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\TroubleshootingSvc /t REG_DWORD /v Start /d 4 /f & " +
                            "reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\DPS /t REG_DWORD /v Start /d 4 /f");
                    }
                    else
                    {
                        TrustedInstaller.CreateProcessAsTrustedInstaller(Settings.PID, "cmd.exe /c reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\WdiSystemHost /t REG_DWORD /v Start /d 3 /f & " +
                            "reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\WdiServiceHost /t REG_DWORD /v Start /d 3 /f & " +
                            "reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\TroubleshootingSvc /t REG_DWORD /v Start /d 3 /f & " +
                            "reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\DPS /t REG_DWORD /v Start /d 2 /f");
                    }
                    break;
                case "TglButton27":
                    if (isChoose)
                    {
                        TrustedInstaller.CreateProcessAsTrustedInstaller(Settings.PID, "cmd.exe /c reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\workfolderssvc /t REG_DWORD /v Start /d 4 /f & " +
                            "reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\dot3svc /t REG_DWORD /v Start /d 4 /f & " +
                            "reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\DevQueryBroker /t REG_DWORD /v Start /d 4 /f & " +
                            "reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\AppMgmt /t REG_DWORD /v Start /d 4 /f");
                    }
                    else
                    {
                        TrustedInstaller.CreateProcessAsTrustedInstaller(Settings.PID, "cmd.exe /c reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\workfolderssvc /t REG_DWORD /v Start /d 3 /f & " +
                            "reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\dot3svc /t REG_DWORD /v Start /d 3 /f & " +
                            "reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\DevQueryBroker /t REG_DWORD /v Start /d 3 /f & " +
                            "reg add HKLM\\SYSTEM\\CurrentControlSet\\Services\\AppMgmt /t REG_DWORD /v Start /d 3 /f");
                    }
                    break;
                case "TglButton28":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\vmicvmsession", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\vmictimesync", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\vmicshutdown", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\vmicrdv", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\vmickvpexchange", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\vmicheartbeat", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\vmicguestinterface", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\HvHost", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\vmicvss", "Start", 4, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\vmicvmsession", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\vmictimesync", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\vmicshutdown", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\vmicrdv", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\vmickvpexchange", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\vmicheartbeat", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\vmicguestinterface", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\HvHost", "Start", 3, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\vmicvss", "Start", 3, RegistryValueKind.DWord);
                    }
                    break;
            }
        }

        private async static void ChangeAccessUpdateFolder(bool isDenyAccess)
        {
            void ChangeStateTask()
            {
                string valueState = isDenyAccess ? "/disable" : "/enable";

                TrustedInstaller.CreateProcessAsTrustedInstaller(Settings.PID, "cmd.exe /c schtasks /change " + valueState + " /tn \"\\Microsoft\\Windows\\UpdateOrchestrator\\Report policies\" & " +
                "schtasks /change " + valueState + " /tn \"\\Microsoft\\Windows\\UpdateOrchestrator\\Schedule Scan\" & " +
                "schtasks /change " + valueState + " /tn \"\\Microsoft\\Windows\\UpdateOrchestrator\\Schedule Scan Static Task\" & " +
                "schtasks /change " + valueState + " /tn \"\\Microsoft\\Windows\\UpdateOrchestrator\\Schedule Work\" & " +
                "schtasks /change " + valueState + " /tn \"\\Microsoft\\Windows\\UpdateOrchestrator\\Start Oobe Expedite Work\" & " +
                "schtasks /change " + valueState + " /tn \"\\Microsoft\\Windows\\UpdateOrchestrator\\StartOobeAppsScanAfterUpdate\" & " +
                "schtasks /change " + valueState + " /tn \"\\Microsoft\\Windows\\UpdateOrchestrator\\StartOobeAppsScan_LicenseAccepted\" & " +
                "schtasks /change " + valueState + " /tn \"\\Microsoft\\Windows\\UpdateOrchestrator\\USO_UxBroker\" & " +
                "schtasks /change " + valueState + " /tn \"\\Microsoft\\Windows\\UpdateOrchestrator\\UUS Failover Task\" & " +
                "schtasks /change " + valueState + " /tn \"\\Microsoft\\Windows\\WindowsUpdate\\Refresh Group Policy Cache\" & " +
                "schtasks /change " + valueState + " /tn \"\\Microsoft\\Windows\\WindowsUpdate\\Scheduled Start\" ");
            }

            try
            {
                if (isDenyAccess)
                {
                    Process.Start(new ProcessStartInfo()
                    {
                        Arguments = @"/c rd /s /q " + Settings.PathSystemDisk + @"\Windows\SoftwareDistribution\Download & 
                            rd /s /q " + Settings.PathSystemDisk + @"\Windows\System32\catroot2",
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true,
                        FileName = "cmd.exe"
                    });

                    ChangeStateTask();

                    await Task.Delay(1000);

                    Process.Start(new ProcessStartInfo()
                    {
                        Arguments = @"/c rd /s /q " + filesPathUpdate + "/(GTweak UpdateOrchestrator) & " +
                        "rd /s /q " + filesPathUpdate + "/(GTweak WindowsUpdate)",
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true,
                        FileName = "cmd.exe"
                    });


                    await Task.Delay(200);

                    if (Directory.Exists(filesPathUpdate + "\\UpdateOrchestrator"))
                        Directory.Move(filesPathUpdate + "\\UpdateOrchestrator", filesPathUpdate + "\\(GTweak UpdateOrchestrator)");
                    if (Directory.Exists(filesPathUpdate + "\\WindowsUpdate"))
                        Directory.Move(filesPathUpdate + "\\WindowsUpdate", filesPathUpdate + "\\(GTweak WindowsUpdate)");
                }
                else
                {

                    if (Directory.Exists(filesPathUpdate + "\\(GTweak UpdateOrchestrator)"))
                        Directory.Move(filesPathUpdate + "\\(GTweak UpdateOrchestrator)", filesPathUpdate + "\\UpdateOrchestrator");
                    if (Directory.Exists(filesPathUpdate + "\\(GTweak WindowsUpdate)"))
                        Directory.Move(filesPathUpdate + "\\(GTweak WindowsUpdate)", filesPathUpdate + "\\WindowsUpdate");

                    await Task.Delay(500);

                    ChangeStateTask();
                }
            } catch (Exception ex) { Debug.WriteLine(ex); }
        }
    }
}
