namespace Unleash.Strategies
{
    using System;
    using System.Collections.Generic;

    public class UserWithIdStrategy : IStrategy
    {
        internal readonly string UserIdsConst = "userIds";

        public string Name => "userWithId";

        public bool IsEnabled(Dictionary<string, string> parameters, UnleashContext context = null)
        {
            var userId = context?.UserId;
            if (userId == null)
                return false;

            if (!parameters.TryGetValue(UserIdsConst, out var userIds))
                return false;

            const string commaDelimeter = ",";
            const string space = " ";

            var idsLocal = string.Concat(commaDelimeter, userIds.Replace(space, string.Empty), commaDelimeter);
            var userLocal = string.Concat(commaDelimeter, userId.Replace(space, string.Empty), commaDelimeter);

            return idsLocal.IndexOf(userLocal, StringComparison.Ordinal) > -1;
        }
    }
}