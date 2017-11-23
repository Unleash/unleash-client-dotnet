namespace Unleash.Strategies
{
    using System.Collections.Generic;

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
        public static string PERCENTAGE = "percentage";
        public static string GROUP_ID = "groupId";

        public string Name => "gradualRolloutUserId";

        public bool IsEnabled(Dictionary<string, string> parameters, UnleashContext context = null)
        {
            var userId = context?.UserId;
            if (userId == null)
                return false;

            var percentageString = parameters[PERCENTAGE];
            var percentage = StrategyUtils.GetPercentage(percentageString);
            var groupId = parameters[GROUP_ID] ?? string.Empty;

            var normalizedUserId = StrategyUtils.GetNormalizedNumber(userId, groupId);

            return percentage > 0 && normalizedUserId <= percentage;
        }
    }
}