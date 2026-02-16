using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
            if (tasklist != null && tasklist.Length != 0)
            {
                bool isEnabledFound = false;

                Parallel.ForEach(tasklist, () => new Microsoft.Win32.TaskScheduler.TaskService(),
                (taskName, loopState, taskScheduler) =>
                {
                    try
                    {
                        if (isEnabledFound)
                        {
                            loopState.Stop();
                            return taskScheduler;
                        }

                        Microsoft.Win32.TaskScheduler.Task scheduledTask = taskScheduler.GetTask(taskName);

                        if (scheduledTask != null && scheduledTask.Enabled)
                        {
                            isEnabledFound = true;
                            loopState.Stop();
                        }
                    }
                    catch (Exception ex) { ErrorLogging.LogDebug(ex); }

                    return taskScheduler;
                },
                taskScheduler =>
                {
                    try { taskScheduler.Dispose(); }
                    catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                });

                return isEnabledFound;
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
                    CommandExecutor.RunCommandAsTrustedInstaller("/c " + CommandExecutor.CleanCommand(string.Join(" & ", existingTasks.Select(task => $"schtasks /change {(state ? "/enable" : "/disable")} /tn \"{task}\""))));
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
                    CommandExecutor.RunCommand("/c " + CommandExecutor.CleanCommand(string.Join(" & ", existingTasks.Select(task => $"schtasks /delete /tn \"{task}\" /f"))));
                }
            });
        }

        internal static string GetTaskFullPath(string partialName)
        {
            string[] files = Directory.GetFiles(PathLocator.Folders.Tasks, "*", SearchOption.AllDirectories);

            List<string> matches = files.Where(path => Path.GetFileName(path).StartsWith(partialName, StringComparison.OrdinalIgnoreCase)).ToList();

            if (matches.Count != 0)
            {
                string relativePath = matches[0].Substring(PathLocator.Folders.Tasks.Length).Replace(Path.DirectorySeparatorChar, '\\');
                relativePath = Regex.Replace(relativePath, @"^\\+", "");

                return $@"\{relativePath}";
            }

            return $@"\{partialName}*";
        }

        internal static string[] GetAllTasksInPaths(params string[] basePaths)
        {
            List<string> taskList = new List<string>();

            foreach (var basePath in basePaths)
            {
                string fullBasePath = Path.Combine(PathLocator.Folders.Tasks, basePath.TrimStart('\\'));

                if (Directory.Exists(fullBasePath))
                {
                    string[] files = Directory.GetFiles(fullBasePath, "*", SearchOption.AllDirectories);

                    foreach (string file in files)
                    {
                        string relativePath = file.Substring(PathLocator.Folders.Tasks.Length).Replace(Path.DirectorySeparatorChar, '\\');
                        relativePath = Regex.Replace(relativePath, @"^\\+", "");
                        taskList.Add(@"\" + relativePath);
                    }
                }
            }

            return taskList.ToArray();
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
