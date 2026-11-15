using Hangfire;
using Hangfire.Storage;

namespace PordznakanAPI.Services
{
    /// <summary>
    /// One lock per synced entity, shared by the Hangfire job and the manual POST endpoints. The lock
    /// lives in the Hangfire SQL storage, so it also holds across app instances and restarts.
    /// Two syncs of the same entity must never overlap: on an empty table both would see "nothing
    /// there yet" and insert the same rows, and they share one staging table.
    /// </summary>
    public static class SyncGuard
    {
        /// <summary>
        /// Takes the lock for <paramref name="entity"/> or returns null at once when another run holds it.
        /// Dispose the result to release the lock.
        /// </summary>
        public static IDisposable? TryAcquire(string entity)
        {
            var connection = JobStorage.Current.GetConnection();
            try
            {
                var distributedLock = connection.AcquireDistributedLock($"emis-sync:{entity}", TimeSpan.Zero);
                return new Releaser(distributedLock, connection);
            }
            catch (DistributedLockTimeoutException)
            {
                connection.Dispose();
                return null;
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }

        private sealed class Releaser : IDisposable
        {
            private readonly IDisposable _lock;
            private readonly IDisposable _connection;

            public Releaser(IDisposable distributedLock, IDisposable connection)
            {
                _lock = distributedLock;
                _connection = connection;
            }

            public void Dispose()
            {
                _lock.Dispose();
                _connection.Dispose();
            }
        }
    }
}
