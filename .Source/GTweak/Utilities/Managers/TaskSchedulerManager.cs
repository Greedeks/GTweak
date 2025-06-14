﻿using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Storage;
using System.Linq;
using System.Threading.Tasks;

namespace GTweak.Utilities.Managers
{
    internal class TaskSchedulerManager : TaskStorage
    {
        internal static bool IsTaskEnabled(params string[] tasklist)
        {
            byte numberRunningTask = 0;
            using (Microsoft.Win32.TaskScheduler.TaskService taskService = new Microsoft.Win32.TaskScheduler.TaskService())
            {
                numberRunningTask += (byte)(from string taskname in tasklist
                                            let task = taskService.GetTask(taskname)
                                            where task != null
                                            where task.Enabled
                                            select taskname).Count();
            }
            return numberRunningTask > 0;
        }

        internal static void SetTaskState(bool state, params string[] tasklist)
        {
            Task.Run(delegate
            {
                using Microsoft.Win32.TaskScheduler.TaskService taskService = new Microsoft.Win32.TaskScheduler.TaskService();
                foreach (string taskname in tasklist)
                {
                    using Microsoft.Win32.TaskScheduler.Task task = taskService.GetTask(taskname);
                    if (task != null)
                    {
                        if (task.Enabled != state)
                        {
                            task.Definition.Settings.Enabled = state;
                            task.RegisterChanges();
                        }
                    }
                }
            });
        }

        internal static void SetTaskStateOwner(bool state, params string[] tasklist)
        {
            Task.Run(delegate
            {
                string command = "cmd.exe /c ";
                foreach (string task in tasklist)
                    command += $"schtasks /change {(state ? "/enable" : "/disable")} /tn \"{task}\" & ";
                command = command.TrimEnd(' ', '&');

                TrustedInstaller.CreateProcessAsTrustedInstaller(SettingsEngine.PID, command);
            });
        }


        internal static void DeletingTask(params string[] tasklist)
        {
            Task.Run(delegate
            {
                using Microsoft.Win32.TaskScheduler.TaskService taskService = new Microsoft.Win32.TaskScheduler.TaskService();
                foreach (string taskname in tasklist)
                {
                    Microsoft.Win32.TaskScheduler.Task task = taskService.GetTask(taskname);
                    if (task != null)
                        taskService.RootFolder.DeleteTask(taskname);
                }
            });
        }
    }
}
