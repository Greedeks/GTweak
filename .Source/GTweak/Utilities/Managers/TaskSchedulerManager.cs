using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Storage;

namespace GTweak.Utilities.Managers
{
    internal class TaskSchedulerManager : TaskStorage
    {
        internal static bool IsTaskEnabled(params string[] tasklist)
        {
            string[] existingTasks = GetExistingTasks(tasklist);

            if (existingTasks.Length != 0)
            {
                byte numberRunningTask = 0;
                using (Microsoft.Win32.TaskScheduler.TaskService taskService = new Microsoft.Win32.TaskScheduler.TaskService())
                {
                    numberRunningTask += (byte)(from string taskname in existingTasks
                                                let task = taskService.GetTask(taskname)
                                                where task != null
                                                where task.Enabled
                                                select taskname).Count();
                }
                return numberRunningTask > 0;
            }
            return false;
        }

        internal static void SetTaskState(bool state, params string[] tasklist)
        {
            Task.Run(delegate
            {
                string[] existingTasks = GetExistingTasks(tasklist);

                if (existingTasks.Length != 0)
                {
                    using Microsoft.Win32.TaskScheduler.TaskService taskService = new Microsoft.Win32.TaskScheduler.TaskService();
                    foreach (string taskname in existingTasks)
                    {
                        using Microsoft.Win32.TaskScheduler.Task task = taskService.GetTask(taskname);
                        if (task != null && task.Enabled != state)
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
            Task.Run(() =>
            {
                string[] existingTasks = GetExistingTasks(tasklist);
                if (existingTasks.Length != 0)
                {
                    IEnumerable<string> commands = existingTasks.Select(task => $"schtasks /change {(state ? "/enable" : "/disable")} /tn \"{task.Replace("\"", "\\\"")}\"");
                    CommandExecutor.RunCommandAsTrustedInstaller($"/c {string.Join(" & ", commands)}");
                }
            });
        }

        internal static void RemoveTasks(params string[] tasklist)
        {
            Task.Run(delegate
            {
                string[] existingTasks = GetExistingTasks(tasklist);

                if (existingTasks.Length != 0)
                {
                    using Microsoft.Win32.TaskScheduler.TaskService taskService = new Microsoft.Win32.TaskScheduler.TaskService();
                    foreach (string taskname in existingTasks)
                    {
                        Microsoft.Win32.TaskScheduler.Task task = taskService.GetTask(taskname);
                        if (task != null)
                        {
                            taskService.RootFolder.DeleteTask(taskname);
                        }
                    }
                }
            });
        }

        internal static string GetTaskFullPath(string partialName)
        {
            string[] files = Directory.GetFiles(PathLocator.Folders.Tasks, "*", SearchOption.AllDirectories);
            string matchPath = files.FirstOrDefault(path => Path.GetFileName(path).IndexOf(partialName, StringComparison.OrdinalIgnoreCase) >= 0);

            if (!string.IsNullOrWhiteSpace(matchPath))
            {
                return Path.GetFileName(matchPath);
            }
            else
            {
                return partialName;
            }
        }

        private static string[] GetExistingTasks(params string[] tasklist)
        {
            List<string> foundExisting = new List<string>(tasklist.Length);

            foreach (string path in tasklist)
            {
                if (File.Exists(Path.Combine(PathLocator.Folders.Tasks, path.TrimStart('\\', '/').Replace('/', '\\'))))
                {
                    foundExisting.Add(path);
                }
            }

            return foundExisting.ToArray();
        }
    }
}
