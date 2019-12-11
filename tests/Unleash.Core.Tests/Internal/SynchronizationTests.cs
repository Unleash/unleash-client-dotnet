using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Unleash.Internal;
using Xunit;

namespace Unleash.Core.Tests.Internal
{
    public class ReaderWriterLockSlimOfTests
    {
        [Theory]
        [InlineData(1000000, 25)]
        public void CurrentReadCount_WhenInvokedInParallel_ShouldNotThrowLockingException(int totalOperations, int maxDegreeOfParallelism)
        {
            var readerWriterLockSlimOf = new ReaderWriterLockSlimOf<object>();

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism,
            };

            var exceptions = new ConcurrentBag<Exception>();
            var result = Parallel.For(0, totalOperations, options, state =>
            {
                try
                {
                    readerWriterLockSlimOf.Instance = state;

                    var i1 = readerWriterLockSlimOf.Instance;
                    var i2 = readerWriterLockSlimOf.Instance;
                    var i3 = readerWriterLockSlimOf.Instance;
                    var i4 = readerWriterLockSlimOf.Instance;
                    var i5 = readerWriterLockSlimOf.Instance;

                    var numReaders = readerWriterLockSlimOf.CurrentReadCount;

                    readerWriterLockSlimOf.Instance = state;

                    var i6 = readerWriterLockSlimOf.Instance;
                    var i7 = readerWriterLockSlimOf.Instance;
                    var i8 = readerWriterLockSlimOf.Instance;
                    var i9 = readerWriterLockSlimOf.Instance;
                    var i10 = readerWriterLockSlimOf.Instance;

                    readerWriterLockSlimOf.Instance = state;
                }
                catch (Exception exc)
                {
                    exceptions.Add(exc);
                }
            });

            Assert.Empty(exceptions);
            Assert.True(result.IsCompleted);
        }
    }
}
