using GTweak.Utilities.Managers;

namespace GTweak.Utilities.Storage
{
    internal class TaskStorage
    {
        protected static readonly string[] dataCollectTasks = {
        @"\Microsoft\Windows\Maintenance\WinSAT",
        @"\Microsoft\Windows\Autochk\Proxy",
        @"\Microsoft\Windows\Application Experience\Microsoft Compatibility Appraiser",
        @"\Microsoft\Windows\Application Experience\ProgramDataUpdater",
        @"\Microsoft\Windows\Application Experience\StartupAppTask",
        @"\Microsoft\Windows\PI\Sqm-Tasks",
        @"\Microsoft\Windows\NetTrace\GatherNetworkInfo",
        @"\Microsoft\Windows\Customer Experience Improvement Program\Consolidator",
        @"\Microsoft\Windows\Customer Experience Improvement Program\KernelCeipTask",
        @"\Microsoft\Windows\Customer Experience Improvement Program\UsbCeip",
        @"\Microsoft\Windows\DiskDiagnostic\Microsoft-Windows-DiskDiagnosticResolver",
        @"\Microsoft\Windows\DiskDiagnostic\Microsoft-Windows-DiskDiagnosticDataCollector" };

        protected static readonly string[] telemetryTasks = {
        @"\Microsoft\Office\Office ClickToRun Service Monitor",
        @"\Microsoft\Office\OfficeTelemetry\AgentFallBack2016",
        @"\Microsoft\Office\OfficeTelemetry\OfficeTelemetryAgentLogOn2016",
        @"\Microsoft\Office\OfficeTelemetryAgentFallBack2016",
        @"\Microsoft\Office\OfficeTelemetryAgentLogOn2016",
        @"\Microsoft\Office\OfficeTelemetryAgentFallBack",
        @"\Microsoft\Office\OfficeTelemetryAgentLogOn",
        @"\Microsoft\Office\Office 15 Subscription Heartbeat" };

        protected static readonly string[] nvidiaTasks = {
        @"\NvTmRepOnLogon_{B2FE1952-0186-46C3-BAEC-A80AA35AC5B8}",
        @"\NvTmRep_{B2FE1952-0186-46C3-BAEC-A80AA35AC5B8}",
        @"\NvTmMon_{B2FE1952-0186-46C3-BAEC-A80AA35AC5B8}" };

        protected static readonly string[] winUpdatesTasks = {
        @"\Microsoft\Windows\UpdateOrchestrator\Report policies",
        @"\Microsoft\Windows\UpdateOrchestrator\Schedule Maintenance Work",
        @"\Microsoft\Windows\UpdateOrchestrator\Schedule Scan",
        @"\Microsoft\Windows\UpdateOrchestrator\Schedule Scan Static Task",
        @"\Microsoft\Windows\UpdateOrchestrator\Schedule Wake To Work",
        @"\Microsoft\Windows\UpdateOrchestrator\Schedule Work",
        @"\Microsoft\Windows\UpdateOrchestrator\Start Oobe Expedite Work",
        @"\Microsoft\Windows\UpdateOrchestrator\StartOobeAppsScanAfterUpdate",
        @"\Microsoft\Windows\UpdateOrchestrator\StartOobeAppsScan_LicenseAccepted",
        @"\Microsoft\Windows\UpdateOrchestrator\UIEOrchestrator",
        @"\Microsoft\Windows\UpdateOrchestrator\USO_UxBroker",
        @"\Microsoft\Windows\UpdateOrchestrator\UUS Failover Task",
        @"\Microsoft\Windows\WindowsUpdate\Refresh Group Policy Cache",
        @"\Microsoft\Windows\WindowsUpdate\Scheduled Start" };

        protected static readonly string[] xboxTasks =  {
        @"\Microsoft\XblGameSave\XblGameSaveTask",
        @"\Microsoft\XblGameSave\XblGameSaveTaskLogon",
        @"\Microsoft\Xbox\XblGameSaveTask",
        @"\Microsoft\Xbox\Maintenance\MaintenanceTask",
        @"\Microsoft\Xbox\XGamingServices\GameServicesTask" };

        protected static readonly string bluetoothTask = @"\Microsoft\Windows\Bluetooth\UninstallDeviceTask";

        protected static readonly string[] mapsTasks =  {
        @"\Microsoft\Windows\Maps\MapsToastTask",
        @"\Microsoft\Windows\Maps\MapsUpdateTask" };

        protected static readonly string[] oneDriveTask = {
        @"\Microsoft\Windows\OneDrive\OneDrive Standalone Update Task",
        $@"\{TaskSchedulerManager.GetTaskFullPath("OneDrive Startup")}" };

        protected static readonly string[] edgeTasks = {
        $@"\{TaskSchedulerManager.GetTaskFullPath("MicrosoftEdgeUpdateTaskMachineUA")}",
        $@"\{TaskSchedulerManager.GetTaskFullPath("MicrosoftEdgeUpdateTaskMachineCore")}",
        $@"\{TaskSchedulerManager.GetTaskFullPath("MicrosoftEdgeUpdateTaskUser")}" };

        protected static readonly string[] memoryDiagTasks =  {
        @"\Microsoft\Windows\MemoryDiagnostic\ProcessMemoryDiagnosticEvents",
        @"\Microsoft\Windows\MemoryDiagnostic\RunFullMemoryDiagnostic" };

        protected static readonly string[] winDefenderTasks = {
        @"\Microsoft\Windows\ExploitGuard\ExploitGuard MDM policy Refresh",
        @"\Microsoft\Windows\Windows Defender\Windows Defender Cache Maintenance",
        @"\Microsoft\Windows\Windows Defender\Windows Defender Cleanup",
        @"\Microsoft\Windows\Windows Defender\Windows Defender Scheduled Scan",
        @"\Microsoft\Windows\Windows Defender\Windows Defender Verification", };

        protected static readonly string restoreTask = @"\Microsoft\Windows\SystemRestore\SR";

        protected static readonly string defragTask = @"\Microsoft\Windows\Defrag\ScheduledDefrag";
    }
}
