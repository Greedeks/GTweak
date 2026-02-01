using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Windows;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Managers;
using Microsoft.Win32;

namespace GTweak.Utilities.Maintenance
{
    internal sealed class SystemRestoreService : TaskSchedulerManager
    {
        [DllImport("srclient.dll")]
        private static extern int DisableSR([MarshalAs(UnmanagedType.LPWStr)] string Drive);
        [DllImport("srclient.dll")]
        private static extern int SRRemoveRestorePoint(int index);
        private readonly ManagementClass _restorePoint = new ManagementClass(new ManagementScope(@"\\localhost\root\default"), new ManagementPath("SystemRestore"), new ObjectGetOptions());
        private ManagementBaseObject _inParams, _outParams;

        internal bool IsPointCreationAllowed => RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore", "RPSessionInterval", "0");

        internal void CreateRestorePoint()
        {
            try
            {
                RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore", "SystemRestorePointCreationFrequency", 0, RegistryValueKind.DWord);

                if (!IsPointCreationAllowed)
                {
                    EnableRestorePoint();
                }

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"\\localhost\root\default", "SELECT Description, SequenceNumber FROM SystemRestore", new EnumerationOptions { ReturnImmediately = true }))
                {
                    foreach (ManagementObject managementObj in searcher.Get().Cast<ManagementObject>())
                    {
                        using (managementObj)
                        {
                            if (managementObj["Description"]?.ToString().IndexOf("GTweak", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                SRRemoveRestorePoint(Convert.ToInt32(managementObj["SequenceNumber"]));
                            }
                        }
                    }
                }

                _inParams = _restorePoint.GetMethodParameters("CreateRestorePoint");
                _inParams["Description"] = (string)Application.Current.Resources["textpoint_more"];
                _inParams["EventType"] = 100;
                _inParams["RestorePointType"] = 12;
                _outParams = _restorePoint.InvokeMethod("CreateRestorePoint", _inParams, null);

                if ((uint)_outParams["ReturnValue"] == 0)
                {
                    NotificationManager.Show("info", "success_point_noty").WithDelay(300).Perform();
                }
                else
                {
                    NotificationManager.Show("warn", "error_point_noty").WithDelay(300).Perform();
                }

                RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore", "SystemRestorePointCreationFrequency");
            }
            catch { NotificationManager.Show("warn", "error_point_noty").Perform(); }
        }

        internal void StartRecovery()
        {
            try
            {
                if (!IsPointCreationAllowed)
                {
                    EnableRestorePoint();
                }

                CommandExecutor.RunCommand("/c rstrui.exe");
            }
            catch { NotificationManager.Show("warn", "error_recovery_noty").Perform(); }
        }

        internal void DisableRestorePoint()
        {
            SetTaskState(false, restoreTask);

            CommandExecutor.RunCommand("/c sc config wbengine start= disabled && sc config swprv start= disabled && sc config vds start= disabled && sc config VSS start= disabled");

            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\SPP\Clients", "{09F7EDC5-294E-4180-AF6A-FB0E6A0E9513}");

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"\\localhost\root\default", "SELECT SequenceNumber FROM SystemRestore", new EnumerationOptions { ReturnImmediately = true }))
            {
                foreach (ManagementObject managementObj in searcher.Get().Cast<ManagementObject>())
                {
                    using (managementObj)
                    {
                        SRRemoveRestorePoint(Convert.ToInt32(managementObj["SequenceNumber"]?.ToString()));
                    }
                }
            }

            DisableSR(PathLocator.Folders.SystemDrive + @"\\");
        }

        internal void EnableRestorePoint()
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
