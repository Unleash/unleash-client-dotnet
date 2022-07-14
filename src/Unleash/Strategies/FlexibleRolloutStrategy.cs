using System;
using System.Collections.Generic;
using Unleash.Internal;

namespace Unleash.Strategies
{
    public class FlexibleRolloutStrategy : IStrategy
    {
        protected static readonly string Percentage = "rollout";
        protected static readonly string GroupId = "groupId";

        public string Name => "flexibleRollout";
        private Func<string> randomGenerator;

        public FlexibleRolloutStrategy()
        {
            var random = new Random();
            randomGenerator = () => (random.Next() * 100).ToString();
        }

        public FlexibleRolloutStrategy(Func<string> randomGenerator)
        {
            this.randomGenerator = randomGenerator;
        }

        public bool IsEnabled(Dictionary<string, string> parameters, UnleashContext context, IEnumerable<Constraint> constraints)
        {
            return StrategyUtils.IsEnabled(this, parameters, context, constraints);
        }

        public bool IsEnabled(Dictionary<string, string> parameters, UnleashContext context)
        {
            var stickiness = GetStickiness(parameters);
            var stickinessId = ResolveStickiness(stickiness, context);
            var percentage = StrategyUtils.GetPercentage(parameters.TryGetValue(Percentage, out var percentageString) ? percentageString : null);
            parameters.TryGetValue(GroupId, out var groupId);

            if (string.IsNullOrEmpty(groupId))
            {
                groupId = "";
            }

            if (!string.IsNullOrEmpty(stickinessId))
            {
                var normalizedUserId = StrategyUtils.GetNormalizedNumber(stickinessId, groupId);
                return percentage > 0 && normalizedUserId <= percentage;
            }
            else
            {
                return false;
            }
        }

        private string GetStickiness(Dictionary<string, string> parameters)
        {
            parameters.TryGetValue("stickiness", out var stickiness);
            return stickiness ?? "default";
        }

        private string ResolveStickiness(string stickiness, UnleashContext context)
        {
            switch (stickiness)
            {
                case "random":
                    return randomGenerator();
                case "default":
                    return context.UserId
                        ?? context.SessionId
                        ?? randomGenerator();
                default:
                    return context.GetByName(stickiness);
            }
        }
    }
}
