using System.Collections.Generic;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Attributes;
using Unleash.Strategies;

namespace Unleash.Benchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<Strategies>();
        }
    }

    public class Strategies
    {
        private readonly IStrategy userWithId = new UserWithIdStrategy();
        private readonly IStrategy gradualRollout = new GradualRolloutUserIdStrategy();

        private readonly Dictionary<string, string> parameters;
        private readonly UnleashContext unleashContext;

        public Strategies()
        {
            parameters = new Dictionary<string, string>()
            {
                {"userIds","seth,demo,gray" },
                {"percentage","50" },
                {"groupId","1" },
            };

            unleashContext = new UnleashContext()
            {
                UserId = "demo"
            };
        }

        [Benchmark]
        public bool UserWithId() => userWithId.IsEnabled(parameters, unleashContext);

        [Benchmark]
        public bool GradualRollout() => gradualRollout.IsEnabled(parameters, unleashContext);
    }
}
