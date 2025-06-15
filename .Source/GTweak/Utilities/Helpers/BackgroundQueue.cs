using System;
using System.Threading;
using System.Threading.Tasks;

namespace GTweak.Utilities.Helpers
{
    internal sealed class BackgroundQueue
    {
        private Task _previousTask = Task.FromResult(true);
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

        internal Task QueueCompleted(Action action)
        {
            lock (_key)
            {
                _previousTask = _previousTask.ContinueWith(t => action(), CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);
                return _previousTask;
            }
        }
    }
}

