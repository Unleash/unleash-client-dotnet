using System;
using System.Threading;

namespace Unleash.Internal
{
    internal class SynchronizationContainer<T> : IDisposable
    {
        private readonly ReaderWriterLockSlim @lock;

        public SynchronizationContainer(LockRecursionPolicy recursionPolicy = LockRecursionPolicy.NoRecursion)
        {
            @lock = new ReaderWriterLockSlim(recursionPolicy);
        }

        private T instance;
        public T Instance
        {
            get
            {
                // Read
                try
                {
                    @lock.EnterReadLock();
                    return instance;
                }
                finally
                {
                    @lock.ExitReadLock();
                }
            }
            set
            {
                // Write
                try
                {
                    @lock.EnterWriteLock();
                    instance = value;
                }
                finally
                {
                    @lock.ExitWriteLock();
                }
            }
        }

        public int CurrentReadCount => @lock.CurrentReadCount;

        public void Dispose()
        {
            @lock?.Dispose();
        }
    }
}