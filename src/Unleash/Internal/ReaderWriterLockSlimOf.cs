using System;
using System.Threading;

namespace Unleash.Internal
{
    internal interface IObjectLock<T> : IDisposable
    {
        /// <summary>
        /// Gets or sets the instance of type T in a thread safe manner
        /// </summary>
        T Instance { get; set; }
    }

    /// <summary>
    /// Provides synchronization control that supports multiple readers and single writer over a given object T.
    /// </summary>
    internal class ReaderWriterLockSlimOf<T> : IObjectLock<T>
    {
        private readonly ReaderWriterLockSlim @lock;

        public ReaderWriterLockSlimOf(LockRecursionPolicy recursionPolicy = LockRecursionPolicy.NoRecursion)
        {
            @lock = new ReaderWriterLockSlim(recursionPolicy);
        }

        private T instance;
        public T Instance
        {
            get
            {
                // Read
                @lock.EnterReadLock();
                try
                {
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
                @lock.EnterWriteLock();
                try
                {
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