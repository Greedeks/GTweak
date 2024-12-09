using System;
using System.Threading;
using System.Threading.Tasks;

namespace GTweak.Utilities.Helpers
{
    internal sealed class BackgroundQueue
    {
        private Task previousTask = Task.FromResult(true);
        private readonly object key = new object();
        internal Task QueueTask(Action action)
        {
            lock (key)
            {
                previousTask = previousTask.ContinueWith(t => action()
                    , CancellationToken.None
                    , TaskContinuationOptions.None
                    , TaskScheduler.Default);
                return previousTask;
            }
        }

        internal Task<T> QueueTask<T>(Func<T> func)
        {
            lock (key)
            {
                var task = previousTask.ContinueWith(t => func()
                    , CancellationToken.None
                    , TaskContinuationOptions.None
                    , TaskScheduler.Default);
                previousTask = task;
                return task;
            }
        }

        internal void QueueCompleted(Action action)
        {
            if (previousTask.IsCompleted)
                action();
        }
    }
}

