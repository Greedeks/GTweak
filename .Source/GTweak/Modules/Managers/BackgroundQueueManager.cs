using System;
using System.Threading;
using System.Threading.Tasks;

namespace GTweak.Modules.Managers
{
    internal sealed class BackgroundQueueManager
    {
        private Task _previousTask = Task.CompletedTask;
        private readonly object _key = new object();

        internal Task QueueTask(Action action)
        {
            lock (_key)
            {
                _previousTask = _previousTask.ContinueWith(t => action(), CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);
                return _previousTask;
            }
        }

        internal Task<T> QueueTask<T>(Func<T> func)
        {
            lock (_key)
            {
                var task = _previousTask.ContinueWith(t => func(), CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);
                _previousTask = task;
                return task;
            }
        }

        internal Task QueueTask(Func<Task> asyncAction)
        {
            lock (_key)
            {
                _previousTask = _previousTask.ContinueWith(t => asyncAction(), CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default).Unwrap();
                return _previousTask;
            }
        }

        internal Task<T> QueueTask<T>(Func<Task<T>> asyncFunc)
        {
            lock (_key)
            {
                var task = _previousTask.ContinueWith(t => asyncFunc(), CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default).Unwrap();
                _previousTask = task;
                return task;
            }
        }

        internal Task WaitForCompletion()
        {
            lock (_key)
            {
                return _previousTask;
            }
        }
    }
}