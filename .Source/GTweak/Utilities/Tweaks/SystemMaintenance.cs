using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Managers;
using Microsoft.Win32;
using System;
using System.IO;
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

                        new NotificationManager(300).Show("info", "success_defrag_on_notification").None();
                    }
                    else
                        new NotificationManager().Show("info", "warn_defrag_on_notification").None();
                }
                else
                {
                    if (IsTaskEnabled(defragTask))
                    {
                        SetTaskStateOwner(false, defragTask);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\services\defragsvc", "Start", 4, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Dfrg\BootOptimizeFunction", "Enable", "N", RegistryValueKind.String);

                        new NotificationManager(300).Show("info", "success_defrag_off_notification").None();
                    }
                    else
                        new NotificationManager().Show("info", "warn_defrag_off_notification").None();
                }
            }
            catch { new NotificationManager().Show("warn", "error_defrag_notification").None(); }
        }


        internal static void CreateRestorePoint()
        {
            try
            {
                RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore", "SystemRestorePointCreationFrequency", 0, RegistryValueKind.DWord);

                EnableRecovery();

                foreach (var managementObj in new ManagementObjectSearcher(@"\\localhost\root\default", "SELECT Description, SequenceNumber FROM SystemRestore", new EnumerationOptions { ReturnImmediately = true }).Get())
                {
                    if (managementObj["Description"].ToString().IndexOf("GTweak", StringComparison.OrdinalIgnoreCase) >= 0)
                        SRRemoveRestorePoint(Convert.ToInt32(managementObj["SequenceNumber"]));
                }

                _inParams = _restorePoint.GetMethodParameters("CreateRestorePoint");
                _inParams["Description"] = (string)Application.Current.Resources["textpoint_more"];
                _inParams["EventType"] = 100;
                _inParams["RestorePointType"] = 12;
                _outParams = _restorePoint.InvokeMethod("CreateRestorePoint", _inParams, null);

                if ((uint)_outParams["ReturnValue"] == 0)
                    new NotificationManager(300).Show("info", "success_point_notification").None();
                else
                    new NotificationManager(300).Show("warn", "error_point_notification").None();

                RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore", "SystemRestorePointCreationFrequency");
            }
            catch { new NotificationManager().Show("warn", "error_point_notification").None(); }
        }

        internal static void StartRecovery()
        {
            try
            {
                EnableRecovery();
                CommandExecutor.RunCommand("/c rstrui.exe");
            }
            catch { new NotificationManager().Show("warn", "error_recovery_notification").None(); }
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

                DisableSR(PathLocator.Folders.SystemDrive + @"\\");
            }
            else
                new NotificationManager().Show("info", "warn_recovery_notification").None();
        }

        private static void EnableRecovery()
        {
            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows NT\SystemRestore");

            SetTaskState(true, restoreTask);

            CommandExecutor.RunCommand("/c sc config wbengine start= demand && sc config swprv start= demand && sc config vds start= demand && sc config VSS start= demand");

            _inParams = _restorePoint.GetMethodParameters("Enable");
            _inParams["WaitTillEnabled"] = true;
            _inParams["Drive"] = Path.GetPathRoot(Environment.SystemDirectory);
            _restorePoint.InvokeMethod("Enable", _inParams, null);
        }
    }
}
