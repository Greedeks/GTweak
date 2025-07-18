using GTweak.Utilities.Helpers;
using GTweak.Utilities.Storage;
using System.Linq;
using System.Text;
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
                    if (task != null && task.Enabled != state)
                    {
                        task.Definition.Settings.Enabled = state;
                        task.RegisterChanges();
                    }
                }
            });
        }

        internal static void SetTaskStateOwner(bool state, params string[] tasklist)
        {
            Task.Run(delegate
            {
                StringBuilder sb = new StringBuilder();
                foreach (string task in tasklist)
                    sb.Append($"schtasks /change {(state ? "/enable" : "/disable")} /tn \"{task}\" & ");
                string tasksCommand = sb.ToString().TrimEnd(' ', '&');

                CommandExecutor.RunCommandAsTrustedInstaller($"/c {tasksCommand}");
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
