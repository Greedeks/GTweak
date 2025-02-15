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

        internal static readonly string[] memoryDiagTasks =  {
        @"Microsoft\Windows\MemoryDiagnostic\ProcessMemoryDiagnosticEvents",
        @"Microsoft\Windows\MemoryDiagnostic\RunFullMemoryDiagnostic" };

        internal static readonly string restoreTask = @"Microsoft\Windows\SystemRestore\SR";
        internal static readonly string defragTask = @"Microsoft\Windows\Defrag\ScheduledDefrag";

        internal static readonly string[] edgeTasks = {
        @"\MicrosoftEdgeUpdateTaskMachineCore",
        @"\MicrosoftEdgeUpdateTaskMachineUA",
        @"\MicrosoftEdgeUpdateTaskUser*" };
    }
}
