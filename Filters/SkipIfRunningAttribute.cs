using Hangfire.Common;
using Hangfire.Server;
using Hangfire.Storage;

namespace PordznakanAPI.Filters
{
    /// <summary>
    /// Like Hangfire's [DisableConcurrentExecution], but a run that finds the same job already
    /// running is skipped straight away instead of waiting for the lock and then failing with
    /// DistributedLockTimeoutException. Used for jobs that are queued by a schedule and at app
    /// start, where an overlap is expected and the running copy already does the work.
    /// </summary>
    public class SkipIfRunningAttribute : JobFilterAttribute, IServerFilter
    {
        private const string LockKey = "SkipIfRunning.Lock";

        public void OnPerforming(PerformingContext filterContext)
        {
            var resource = $"{filterContext.BackgroundJob.Job.Type.FullName}.{filterContext.BackgroundJob.Job.Method.Name}";

            try
            {
                // Zero timeout: take the lock now or not at all.
                filterContext.Items[LockKey] = filterContext.Connection.AcquireDistributedLock(resource, TimeSpan.Zero);
            }
            catch (DistributedLockTimeoutException)
            {
                // Another run holds the lock. Cancelling here makes Hangfire skip the method
                // and finish this job without an error.
                filterContext.Canceled = true;
            }
        }

        public void OnPerformed(PerformedContext filterContext)
        {
            if (filterContext.Items.TryGetValue(LockKey, out var lockObject) && lockObject is IDisposable distributedLock)
                distributedLock.Dispose();
        }
    }
}
