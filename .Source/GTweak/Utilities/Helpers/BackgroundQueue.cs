﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace GTweak.Utilities.Helpers
{
    internal class BackgroundQueue
    {
        private Task previousTask = Task.FromResult(true);
        private readonly object key = new object();
        public Task QueueTask(Action action)
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

        public Task<T> QueueTask<T>(Func<T> work)
        {
            lock (key)
            {
                var task = previousTask.ContinueWith(t => work()
                    , CancellationToken.None
                    , TaskContinuationOptions.None
                    , TaskScheduler.Default);
                previousTask = task;
                return task;
            }
        }
        public bool IsQueueCompleted() => previousTask.IsCompleted;
    }
}
