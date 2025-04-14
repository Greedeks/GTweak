using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Helpers.Managers;
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
        private static readonly ManagementClass _restorePoint = new ManagementClass(new ManagementScope(@"\\localhost\root\default"), new ManagementPath("SystemRestore"), new ObjectGetOptions());
        private static ManagementBaseObject _inParams, _outParams;
        private static bool _isWorkingCreatePoint = false;

        internal static bool IsSystemRestoreDisabled => RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore", "RPSessionInterval", "0");

        internal static void SetDefragState(bool enable)
        {
            try
            {
                if (enable)
                {
                    if (!IsTaskEnabled(defragTask))
                    {
                        SetTaskStateOwner(true, defragTask);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\services\defragsvc", "Start", 2, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Dfrg\BootOptimizeFunction", "Enable", "Y", RegistryValueKind.String);

                        new ViewNotification(300).Show("", "info", "success_defrag_on_notification");
                    }
                    else
                        new ViewNotification().Show("", "info", "warn_defrag_on_notification");
                }
                else
                {
                    if (IsTaskEnabled(defragTask))
                    {
                        SetTaskStateOwner(false, defragTask);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\services\defragsvc", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Dfrg\BootOptimizeFunction", "Enable", "N", RegistryValueKind.String);

                        new ViewNotification(300).Show("", "info", "success_defrag_off_notification");
                    }
                    else
                        new ViewNotification().Show("", "info", "warn_defrag_off_notification");
                }
            }
            catch { new ViewNotification().Show("", "warn", "error_defrag_notification"); }
        }


        internal static void CreateRestorePoint()
        {
            if (_isWorkingCreatePoint) return;

            try
            {
                _isWorkingCreatePoint = true;
                RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore", "SystemRestorePointCreationFrequency", 0, RegistryValueKind.DWord);

                EnableRecovery();

                foreach (var managementObj in new ManagementObjectSearcher(@"\\localhost\root\default", "SELECT Description, SequenceNumber FROM SystemRestore", new EnumerationOptions { ReturnImmediately = true }).Get())
                {
                    if (managementObj["Description"].ToString().Contains("GTweak"))
                        SRRemoveRestorePoint(Convert.ToInt32(managementObj["SequenceNumber"]));
                }

                _inParams = _restorePoint.GetMethodParameters("CreateRestorePoint");
                _inParams["Description"] = (string)Application.Current.Resources["textpoint_more"];
                _inParams["EventType"] = 100;
                _inParams["RestorePointType"] = 12;
                _outParams = _restorePoint.InvokeMethod("CreateRestorePoint", _inParams, null);

                if ((uint)_outParams["ReturnValue"] == 0)
                    new ViewNotification(300).Show("", "info", "successpoint_notification");
                else
                    new ViewNotification(300).Show("", "warn", "notsuccessfulpoint_notification");

                _isWorkingCreatePoint = false;
                RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore", "SystemRestorePointCreationFrequency");
            }
            catch
            {
                _isWorkingCreatePoint = false;
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

            _inParams = _restorePoint.GetMethodParameters("Enable");
            _inParams["WaitTillEnabled"] = true;
            _inParams["Drive"] = System.IO.Path.GetPathRoot(Environment.SystemDirectory);
            _restorePoint.InvokeMethod("Enable", _inParams, null);
        }
    }
}
