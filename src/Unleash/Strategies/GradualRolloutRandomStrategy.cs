namespace Unleash.Strategies
{
    using System;
    using System.Collections.Generic;

    public class GradualRolloutRandomStrategy : IStrategy
    {
        protected static string PERCENTAGE = "percentage";
        private static string STRATEGY_NAME = "gradualRolloutRandom";

        private readonly Random random;

        public GradualRolloutRandomStrategy()
        {
            random = new Random();
        }

        public GradualRolloutRandomStrategy(int seed)
        {
            random = new Random(seed);
        }

        public string Name => STRATEGY_NAME;

        public bool IsEnabled(Dictionary<string, string> parameters, UnleashContext context = null)
        {
            if (!parameters.TryGetValue(PERCENTAGE, out var value))
                return false;

            var percentage = StrategyUtils.GetPercentage(value);
            var randomNumber = random.Next(100) + 1;

            return percentage >= randomNumber;
        }
    }
}