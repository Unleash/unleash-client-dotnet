using System.Threading;

namespace Unleash.Repository
{
    internal class ToggleCollectionInstance
    {
        private ToggleCollection toggleCollection = new ToggleCollection();

        public ToggleCollectionInstance(UnleashConfig config)
        {
            if (config.InMemoryTogglesForUnitTestingPurposes != null)
            {
                Update(config.InMemoryTogglesForUnitTestingPurposes);
            }
            else
            {
                using (var fileStream = config.Services.FileSystem.FileOpenRead(config.BackupFile))
                {
                    var collection = config.Services.JsonSerializer.Deserialize<ToggleCollection>(fileStream);
                    if (collection == null)
                        return;

                    Update(collection);
                }
            }

        }

        private static readonly ReaderWriterLockSlim ReaderWriterLock = new ReaderWriterLockSlim();

        public ToggleCollection ToggleCollection
        {
            get
            {
                ReaderWriterLock.EnterReadLock();
                try
                {
                    return toggleCollection;
                }
                finally
                {
                    ReaderWriterLock.ExitReadLock();
                }
            }
        }

        public void Update(ToggleCollection toggleCollection)
        {
            ReaderWriterLock.EnterWriteLock();
            try
            {
                this.toggleCollection = toggleCollection;
            }
            finally
            {
                ReaderWriterLock.ExitWriteLock();
            }
        }
    }
}