using System.Linq;
using System.Threading.Tasks;

namespace GTweak.Utilities.Helpers
{
    internal class TaskSchedulerManager
    {
        internal bool IsTaskEnabled(string[] tasklist)
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

        internal static void EnablingTasks(string[] tasklist)
        {
            Parallel.Invoke(() =>
            {
                using Microsoft.Win32.TaskScheduler.TaskService taskService = new Microsoft.Win32.TaskScheduler.TaskService();
                foreach (string taskname in tasklist)
                {
                    using Microsoft.Win32.TaskScheduler.Task task = taskService.GetTask(taskname);
                    if (task != null)
                    {
                        if (!task.Enabled)
                        {
                            task.Definition.Settings.Enabled = true;
                            task.RegisterChanges();
                        }
                    }
                }
            });
        }

        internal static void DisablingTasks(string[] tasklist)
        {
            Parallel.Invoke(() =>
            {
                using Microsoft.Win32.TaskScheduler.TaskService taskService = new Microsoft.Win32.TaskScheduler.TaskService();
                foreach (string taskname in tasklist)
                {
                    using Microsoft.Win32.TaskScheduler.Task task = taskService.GetTask(taskname);
                    if (task != null)
                    {
                        if (task.Enabled)
                        {
                            task.Definition.Settings.Enabled = false;
                            task.RegisterChanges();
                        }
                    }
                }
            });
        }
    }
}
