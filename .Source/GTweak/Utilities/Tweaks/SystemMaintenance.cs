using GTweak.Utilities.Control;
using GTweak.Utilities.Helpers;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace GTweak.Utilities.Tweaks
{
    internal class SystemMaintenance : TaskSchedulerManager
    {
        [DllImport("srclient.dll")]
        public static extern int DisableSR([MarshalAs(UnmanagedType.LPWStr)] string Drive);
        [DllImport("srclient.dll")]
        private static extern int SRRemoveRestorePoint(int index);
        private static readonly ManagementClass RestorePoint = new ManagementClass(new ManagementScope(@"\\localhost\root\default"), new ManagementPath("SystemRestore"), new ObjectGetOptions());
        private static ManagementBaseObject InParams, OutParams;
        private static bool isWorkingCreatePoint = false;
        private static string output = string.Empty;
        private static readonly string[] RestoreTask = { @"Microsoft\Windows\SystemRestore\SR" };
        private readonly string[] DefragTask = { @"Microsoft\Windows\Defrag\ScheduledDefrag" };

        internal static bool IsSystemRestoreDisabled => RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore", "RPSessionInterval", "0");

        internal void DisableDefrag()
        {
            if (IsTaskEnabled(DefragTask))
            {
                try
                {
                    SetTaskStateOwner(DefragTask, false);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\services\defragsvc", "Start", 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Dfrg\BootOptimizeFunction", "Enable", "N", RegistryValueKind.String);

                    new ViewNotification(300).Show("", "info", (string)Application.Current.Resources["success_defrag_notification"]);
                }
                catch { new ViewNotification(300).Show("", "warn", (string)Application.Current.Resources["error_defrag_notification"]); }
            }
            else
                new ViewNotification(300).Show("", "info", (string)Application.Current.Resources["warn_defrag_notification"]);
        }

        internal static async void CreateRestorePoint(string description)
        {
            if (isWorkingCreatePoint) return;

            isWorkingCreatePoint = true;
            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore", "SystemRestorePointCreationFrequency", 0, RegistryValueKind.DWord);

            try
            {
                EnableRecovery();

                await StartPowerShell(@"Get-ComputerRestorePoint | Where-Object {$_.Description -like '*GTweak*'} | Select-Object -ExpandProperty SequenceNumber");

                if (!string.IsNullOrEmpty(output))
                    SRRemoveRestorePoint(Convert.ToInt32(output.Trim()));

                InParams = RestorePoint.GetMethodParameters("CreateRestorePoint");
                InParams["Description"] = description;
                InParams["EventType"] = 100;
                InParams["RestorePointType"] = 12;
                OutParams = RestorePoint.InvokeMethod("CreateRestorePoint", InParams, null);

                if ((uint)OutParams["ReturnValue"] == 0)
                    new ViewNotification(300).Show("", "info", (string)Application.Current.Resources["successpoint_notification"]);
                else
                    new ViewNotification(300).Show("", "warn", (string)Application.Current.Resources["notsuccessfulpoint_notification"]);

                isWorkingCreatePoint = false;
                RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore", "SystemRestorePointCreationFrequency");
            }
            catch { new ViewNotification(300).Show("", "warn", (string)Application.Current.Resources["notsuccessfulpoint_notification"]); }
        }

        internal static void StartRecovery()
        {
            EnableRecovery();

            Process.Start(new ProcessStartInfo()
            {
                Arguments = "/c rstrui.exe",
                FileName = "cmd.exe",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            });
        }

        internal static void DisableRestorePoint()
        {
            SetTaskState(RestoreTask, false);

            Process.Start(new ProcessStartInfo()
            {
                Arguments = "/c sc config wbengine start= disabled && " +
                    "reg delete \"HKLM\\Software\\Microsoft\\Windows NT\\CurrentVersion\\SPP\\Clients\" /v \"{09F7EDC5 - 294E-4180 - AF6A - FB0E6A0E9513}\" /f &&" +
                    "sc config swprv start= disabled && sc config vds start= disabled && sc config VSS start= disabled",
                FileName = "cmd.exe",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            });

            EnumerationOptions optionsObj = new EnumerationOptions { ReturnImmediately = true };
            foreach (var managementObj in new ManagementObjectSearcher(@"\\localhost\root\default", "SELECT SequenceNumber FROM SystemRestore", optionsObj).Get())
                SRRemoveRestorePoint(Convert.ToInt32(managementObj["SequenceNumber"].ToString()));

            DisableSR(UsePath.SystemDisk + @"\\");
        }

        private static void EnableRecovery()
        {
            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows NT\SystemRestore");

            SetTaskState(RestoreTask, true);

            Process.Start(new ProcessStartInfo()
            {
                Arguments = "/c sc config wbengine start= demand && " +
                    "reg add \"HKLM\\Software\\Microsoft\\Windows NT\\CurrentVersion\\SPP\\Clients\" /v \"{09F7EDC5 - 294E-4180 - AF6A - FB0E6A0E9513}\" /t REG_MULTI_SZ /d \"1\" /f &&" +
                    "sc config swprv start= demand && sc config vds start= demand && sc config VSS start= demand",
                FileName = "cmd.exe",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            });

            InParams = RestorePoint.GetMethodParameters("Enable");
            InParams["WaitTillEnabled"] = true;
            InParams["Drive"] = System.IO.Path.GetPathRoot(Environment.SystemDirectory);
            RestorePoint.InvokeMethod("Enable", InParams, null);
        }

        private static async Task<string> StartPowerShell(string arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process process = new Process { StartInfo = startInfo };

            process.Start();

            Task<string> outputTask = process.StandardOutput.ReadToEndAsync();

            await Task.WhenAll(outputTask);

            output = await outputTask;

            return output;
        }
    }
}
