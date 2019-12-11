using Unleash.Internal;

namespace Unleash.Strategies
{
    using System;
    using System.Collections.Generic;

    public class GradualRolloutRandomStrategy : IStrategy
    {
        private static readonly string Percentage = "percentage";
        private static readonly string StrategyName = "gradualRolloutRandom";

        private readonly IRandom random;

        public GradualRolloutRandomStrategy(IRandom random)
        {
            this.random = random;
        }

        public string Name => StrategyName;

        public bool IsEnabled(Dictionary<string, string> parameters, UnleashContext context)
        {
            if (!parameters.TryGetValue(Percentage, out var value))
                return false;

            var percentage = StrategyUtils.GetPercentage(value);
            var randomNumber = random.Next(100) + 1;

            return percentage >= randomNumber;
        }
    }
}
