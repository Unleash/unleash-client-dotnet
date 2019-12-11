using System.Linq;

namespace Unleash.Strategies
{
    using System;
    using System.Collections.Generic;

    /// <inheritdoc />
    public class UserWithIdStrategy : IStrategy
    {
        internal readonly string UserIdsConst = "userIds";

        /// <inheritdoc />
        public string Name => "userWithId";

        /// <inheritdoc />
        public bool IsEnabled(Dictionary<string, string> parameters, UnleashContext context )
        {
            var userId = context?.UserId;
            if (string.IsNullOrEmpty(userId))
                return false;

            if (!parameters.TryGetValue(UserIdsConst, out var userIds))
                return false;

            return userIds.Split(',').Select(x => x.Trim()).Contains(userId.Trim(), StringComparer.Ordinal);
        }
    }
}
