using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Unleash.Internal;

namespace Unleash.Tests.Synchronization
{
    public class SynchronizationTests
    {
        [Test]
        public void ShouldNotThrowLockingException()
        {
            var x = new ReaderWriterLockSlimOf<object>();

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = 25,
            };

            var result = Parallel.For(0, 1000 * 1000, options, state =>
            {
                x.Instance = state;
                var i1 = x.Instance;
                var i2 = x.Instance;
                var i3 = x.Instance;
                var i4 = x.Instance;
                var i5 = x.Instance;

                var numReaders = x.CurrentReadCount;
                if (numReaders > 1)
                    Console.WriteLine($"Num Readers: {numReaders}");

                x.Instance = state;

                var i6 = x.Instance;
                var i7 = x.Instance;
                var i8 = x.Instance;
                var i9 = x.Instance;
                var i10 = x.Instance;
                x.Instance = state;

            });

            Console.WriteLine(x.Instance);
            result.IsCompleted.Should().BeTrue();
        }
    }
}