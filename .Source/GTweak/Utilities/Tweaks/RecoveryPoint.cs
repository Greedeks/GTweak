using GTweak.Utilities.Helpers;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GTweak.Utilities.Tweaks
{
    internal sealed class RecoveryPoint
    {
        [DllImport("srclient.dll")]
        public static extern int DisableSR([MarshalAs(UnmanagedType.LPWStr)] string Drive);
        [DllImport("srclient.dll")]
        private static extern int SRRemoveRestorePoint(int index);
        private static readonly ManagementClass RestorePoint = new ManagementClass(new ManagementScope(@"\\localhost\root\default"), new ManagementPath("SystemRestore"), new ObjectGetOptions());
        private static ManagementBaseObject InParams;
        private static bool isWorkingCreatePoint = false;
        private static string resultRead = default;

        internal static bool IsAlreadyPoint()
        {
            StartPowerShell(@"Get-ComputerRestorePoint | Where-Object {$_.Description -ne 'Точка созданная с помощью GTweak' -and $_.Description -ne 'A point created with a GTweak'} | Select-Object EventType | ft -hide");
            return resultRead.Contains("100");
        }


        private static void EnablePoint()
        {
            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows NT\SystemRestore");

            Microsoft.Win32.TaskScheduler.TaskService _taskService = new Microsoft.Win32.TaskScheduler.TaskService();
            Microsoft.Win32.TaskScheduler.Task _task = _taskService.GetTask(@"Microsoft\Windows\SystemRestore\SR");

            if (_task != null)
            {
                if (_task.Enabled)
                {
                    _task.Definition.Settings.Enabled = true;
                    _task.RegisterChanges();
                }
            }

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


        internal static async void Create(string description)
        {
            if (isWorkingCreatePoint) return;

            isWorkingCreatePoint = true;

            EnablePoint();

            if (StartPowerShell(@"Get-ComputerRestorePoint | Where-Object {$_.Description -eq 'Точка созданная с помощью GTweak' -or $_.Description -eq 'A point created with a GTweak'} | Select-Object SequenceNumber | ft -hide") != string.Empty)
                SRRemoveRestorePoint(Convert.ToInt32(resultRead.Trim()));

            InParams = RestorePoint.GetMethodParameters("CreateRestorePoint");
            InParams["Description"] = description;
            InParams["EventType"] = 100;
            InParams["RestorePointType"] = 12;
            RestorePoint.InvokeMethod("CreateRestorePoint", InParams, null);

            await Task.Delay(200);

            RestorePoint.Dispose();

            isWorkingCreatePoint = false;
        }

        internal static void Run()
        {
            EnablePoint();

            Process.Start(new ProcessStartInfo()
            {
                Arguments = "/c rstrui.exe",
                FileName = "cmd.exe",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            });
        }

        internal static bool IsSystemRestoreDisabled()
        {
            try
            {
                return Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore", "RPSessionInterval", string.Empty).ToString() switch
                {
                    "0" => true,
                    _ => false
                };
            } catch { return false; }
        }

        internal static void DisablePoint()
        {
            Microsoft.Win32.TaskScheduler.TaskService _taskService = new Microsoft.Win32.TaskScheduler.TaskService();
            Microsoft.Win32.TaskScheduler.Task _task = _taskService.GetTask(@"Microsoft\Windows\SystemRestore\SR");

            if (_task != null)
            {
                if (_task.Enabled)
                {
                    _task.Definition.Settings.Enabled = false;
                    _task.RegisterChanges();
                }
            }

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

            DisableSR(Settings.PathSystemDisk + @"\\");
        }

        private static string StartPowerShell(string _arguments)
        {
            Parallel.Invoke(() =>
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = @"powershell.exe",
                    Arguments = _arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process process = new Process() { StartInfo = startInfo, EnableRaisingEvents = true };

                process.Start();

                resultRead = process.StandardOutput.ReadToEnd();

                process.Close();
                process.Dispose();
            });

            return resultRead;
        }
    }
}
