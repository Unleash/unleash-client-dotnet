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
            var instance = new ReaderWriterLockSlimOf<object>();

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = 25,
            };

            var result = Parallel.For(0, 1000 * 1000, options, state =>
            {
                instance.Instance = state;
                var i1 = instance.Instance;
                var i2 = instance.Instance;
                var i3 = instance.Instance;
                var i4 = instance.Instance;
                var i5 = instance.Instance;

                var numReaders = instance.CurrentReadCount;
                if (numReaders > 1)
                    Console.WriteLine($"Num Readers: {numReaders}");

                instance.Instance = state;

                var i6 = instance.Instance;
                var i7 = instance.Instance;
                var i8 = instance.Instance;
                var i9 = instance.Instance;
                var i10 = instance.Instance;
                instance.Instance = state;

            });

            Console.WriteLine(instance.Instance);
            result.IsCompleted.Should().BeTrue();
        }
    }
}