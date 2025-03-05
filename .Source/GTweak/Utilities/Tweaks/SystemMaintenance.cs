using GTweak.Utilities.Control;
using GTweak.Utilities.Helpers;
using Microsoft.Win32;
using System;
using System.Management;
using System.Runtime.InteropServices;
using System.Windows;

namespace GTweak.Utilities.Tweaks
{
    internal sealed class SystemMaintenance : TaskSchedulerManager
    {
        [DllImport("srclient.dll")]
        private static extern int DisableSR([MarshalAs(UnmanagedType.LPWStr)] string Drive);
        [DllImport("srclient.dll")]
        private static extern int SRRemoveRestorePoint(int index);
        private static readonly ManagementClass RestorePoint = new ManagementClass(new ManagementScope(@"\\localhost\root\default"), new ManagementPath("SystemRestore"), new ObjectGetOptions());
        private static ManagementBaseObject InParams, OutParams;
        private static bool isWorkingCreatePoint = false;

        internal static bool IsSystemRestoreDisabled => RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore", "RPSessionInterval", "0");

        internal static void DisableDefrag()
        {
            if (IsTaskEnabled(defragTask))
            {
                try
                {
                    SetTaskStateOwner(false, defragTask);
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\services\defragsvc", "Start", 4, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Dfrg\BootOptimizeFunction", "Enable", "N", RegistryValueKind.String);

                    new ViewNotification(300).Show("", "info", "success_defrag_notification");
                }
                catch { new ViewNotification().Show("", "warn", "error_defrag_notification"); }
            }
            else
                new ViewNotification().Show("", "info", "warn_defrag_notification");
        }

        internal static void CreateRestorePoint()
        {
            if (isWorkingCreatePoint) return;

            try
            {
                isWorkingCreatePoint = true;
                RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore", "SystemRestorePointCreationFrequency", 0, RegistryValueKind.DWord);

                EnableRecovery();

                foreach (var managementObj in new ManagementObjectSearcher(@"\\localhost\root\default", "SELECT Description, SequenceNumber FROM SystemRestore", new EnumerationOptions { ReturnImmediately = true }).Get())
                {
                    if (managementObj["Description"].ToString().Contains("GTweak"))
                        SRRemoveRestorePoint(Convert.ToInt32(managementObj["SequenceNumber"]));
                }

                InParams = RestorePoint.GetMethodParameters("CreateRestorePoint");
                InParams["Description"] = (string)Application.Current.Resources["textpoint_more"];
                InParams["EventType"] = 100;
                InParams["RestorePointType"] = 12;
                OutParams = RestorePoint.InvokeMethod("CreateRestorePoint", InParams, null);

                if ((uint)OutParams["ReturnValue"] == 0)
                    new ViewNotification(300).Show("", "info", "successpoint_notification");
                else
                    new ViewNotification(300).Show("", "warn", "notsuccessfulpoint_notification");

                isWorkingCreatePoint = false;
                RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore", "SystemRestorePointCreationFrequency");
            }
            catch
            {
                isWorkingCreatePoint = false;
                new ViewNotification().Show("", "warn", "notsuccessfulpoint_notification");
            }
        }

        internal static void StartRecovery()
        {
            try
            {
                EnableRecovery();
                CommandExecutor.RunCommand("/c rstrui.exe");
            }
            catch { new ViewNotification().Show("", "warn", "notsuccessfulrecovery_notification"); }
        }

        internal static void DisableRestorePoint()
        {
            if (IsSystemRestoreDisabled)
            {
                SetTaskState(false, restoreTask);

                CommandExecutor.RunCommand("/c sc config wbengine start= disabled && sc config swprv start= disabled && sc config vds start= disabled && sc config VSS start= disabled");

                RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\SPP\Clients", "{09F7EDC5-294E-4180-AF6A-FB0E6A0E9513}");

                foreach (var managementObj in new ManagementObjectSearcher(@"\\localhost\root\default", "SELECT SequenceNumber FROM SystemRestore", new EnumerationOptions { ReturnImmediately = true }).Get())
                    SRRemoveRestorePoint(Convert.ToInt32(managementObj["SequenceNumber"].ToString()));

                DisableSR(StoragePaths.SystemDisk + @"\\");
            }
            else
                new ViewNotification().Show("", "info", "warndisable_recovery_notification");
        }

        private static void EnableRecovery()
        {
            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows NT\SystemRestore");

            SetTaskState(true, restoreTask);

            CommandExecutor.RunCommand("/c sc config wbengine start= demand && sc config swprv start= demand && sc config vds start= demand && sc config VSS start= demand");

            InParams = RestorePoint.GetMethodParameters("Enable");
            InParams["WaitTillEnabled"] = true;
            InParams["Drive"] = System.IO.Path.GetPathRoot(Environment.SystemDirectory);
            RestorePoint.InvokeMethod("Enable", InParams, null);
        }
    }
}
