namespace Unleash.Strategies
{
    using System;
    using System.Collections.Generic;
    using Unleash.Internal;

    public class GradualRolloutRandomStrategy : IStrategy
    {
        private static readonly string Percentage = "percentage";
        private static readonly string StrategyName = "gradualRolloutRandom";

        private readonly Random random;

        public GradualRolloutRandomStrategy()
        {
            random = new Random(Guid.NewGuid().GetHashCode());
        }

        public GradualRolloutRandomStrategy(int seed)
        {
            random = new Random(seed);
        }

        public string Name => StrategyName;

        public bool IsEnabled(Dictionary<string, string> parameters, UnleashContext context = null)
        {
            if (!parameters.TryGetValue(Percentage, out var value))
                return false;

            var percentage = StrategyUtils.GetPercentage(value);
            var randomNumber = random.Next(100) + 1;

            return percentage >= randomNumber;
        }

        public bool IsEnabled(Dictionary<string, string> parameters, UnleashContext context, IEnumerable<Constraint> constraints)
        {
            return StrategyUtils.IsEnabled(this, parameters, context, constraints);
        }
    }
}