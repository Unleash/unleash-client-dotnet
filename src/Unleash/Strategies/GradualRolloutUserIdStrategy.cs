namespace Unleash.Strategies
{
    using System.Collections.Generic;
    using Unleash.Internal;

    /**
     * : a gradual roll-out strategy based on userId.
     *
     * Using this strategy you can target only logged in users and gradually expose your
     * feature to higher percentage of the logged in user.
     *
     * This strategy takes two parameters:
     *  - percentage :  a number between 0 and 100. The percentage you want to enable the feature for.
     *  - groupId :     a groupId used for rolling out the feature. By using the same groupId for different
     *                  toggles you can correlate the user experience across toggles.
     *
     */
    public class GradualRolloutUserIdStrategy : IStrategy
    {
        public static readonly string PercentageConst = "percentage";
        public static readonly string GroupIdConst = "groupId";

        public string Name => "gradualRolloutUserId";

        public bool IsEnabled(Dictionary<string, string> parameters, UnleashContext context = null)
        {
            var userId = context?.UserId;
            if (userId == null || userId == string.Empty)
                return false;

            if (!(parameters.ContainsKey(PercentageConst) && parameters.ContainsKey(GroupIdConst)))
                return false;

            var percentageString = parameters[PercentageConst];
            var percentage = StrategyUtils.GetPercentage(percentageString);
            var groupId = parameters[GroupIdConst] ?? string.Empty;

            var normalizedUserId = StrategyUtils.GetNormalizedNumber(userId, groupId);

            return percentage > 0 && normalizedUserId <= percentage;
        }

        public bool IsEnabled(Dictionary<string, string> parameters, UnleashContext context, IEnumerable<Constraint> constraints)
        {
            return StrategyUtils.IsEnabled(this, parameters, context, constraints);
        }
    }
}