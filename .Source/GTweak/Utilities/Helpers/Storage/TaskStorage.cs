namespace GTweak.Utilities.Helpers.Storage
{
    internal class TaskStorage
    {
        internal static readonly string[] dataCollectTasks = {
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
        @"Microsoft\Windows\DiskDiagnostic\Microsoft-Windows-DiskDiagnosticDataCollector" };

        internal static readonly string[] nvidiaTasks = {
        @"NvTmRepOnLogon_{B2FE1952-0186-46C3-BAEC-A80AA35AC5B8}",
        @"NvTmRep_{B2FE1952-0186-46C3-BAEC-A80AA35AC5B8}",
        @"NvTmMon_{B2FE1952-0186-46C3-BAEC-A80AA35AC5B8}" };

        internal static readonly string[] telemetryTasks = {
        @"Microsoft\Office\Office ClickToRun Service Monitor",
        @"Microsoft\Office\OfficeTelemetry\AgentFallBack2016",
        @"Microsoft\Office\OfficeTelemetry\OfficeTelemetryAgentLogOn2016",
        @"Microsoft\Office\OfficeTelemetryAgentFallBack2016",
        @"Microsoft\Office\OfficeTelemetryAgentLogOn2016",
        @"Microsoft\Office\OfficeTelemetryAgentFallBack",
        @"Microsoft\Office\OfficeTelemetryAgentLogOn",
        @"Microsoft\Office\Office 15 Subscription Heartbeat" };

        internal static readonly string[] winUpdatesTasks = {
        @"Microsoft\Windows\UpdateOrchestrator\Report policies",
        @"Microsoft\Windows\UpdateOrchestrator\Schedule Scan",
        @"Microsoft\Windows\UpdateOrchestrator\Schedule Scan Static Task",
        @"Microsoft\Windows\UpdateOrchestrator\Schedule Work",
        @"Microsoft\Windows\UpdateOrchestrator\Start Oobe Expedite Work",
        @"Microsoft\Windows\UpdateOrchestrator\StartOobeAppsScanAfterUpdate",
        @"Microsoft\Windows\UpdateOrchestrator\StartOobeAppsScan_LicenseAccepted",
        @"Microsoft\Windows\UpdateOrchestrator\USO_UxBroker",
        @"Microsoft\Windows\UpdateOrchestrator\UUS Failover Task",
        @"Microsoft\Windows\WindowsUpdate\Refresh Group Policy Cache",
        @"Microsoft\Windows\WindowsUpdate\Scheduled Start"};

        internal static readonly string[] memoryDiagTasks =  {
        @"Microsoft\Windows\MemoryDiagnostic\ProcessMemoryDiagnosticEvents",
        @"Microsoft\Windows\MemoryDiagnostic\RunFullMemoryDiagnostic" };

        internal static readonly string restoreTask = @"Microsoft\Windows\SystemRestore\SR";
        internal static readonly string defragTask = @"Microsoft\Windows\Defrag\ScheduledDefrag";

        internal static readonly string[] edgeTasks = {
        @"MicrosoftEdgeUpdateTaskMachineUA{EA17DF76-AE5F-45F8-8867-FE5E0DD06656}",
        @"MicrosoftEdgeUpdateTaskMachineCore{11EED34D-1A62-4BB7-9A2E-0D83ED6A609F}",
        @"MicrosoftEdgeUpdateTaskUser*" };
    }
}
