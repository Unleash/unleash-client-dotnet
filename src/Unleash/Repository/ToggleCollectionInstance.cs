using System.Threading;
using Unleash.Serialization;
using Unleash.Util;

namespace Unleash.Repository
{
    internal class ToggleCollectionInstance
    {
        private ToggleCollection toggleCollection = new ToggleCollection();

        public ToggleCollectionInstance(IJsonSerializer jsonSerializer, IFileSystem fileSystem, string toggleFile)
        {
            using (var fileStream = fileSystem.FileOpenRead(toggleFile))
            {
                var collection = jsonSerializer.Deserialize<ToggleCollection>(fileStream);
                if (collection == null)
                    return;

                Update(collection);
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