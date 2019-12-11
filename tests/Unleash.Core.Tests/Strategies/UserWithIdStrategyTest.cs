using System.Collections.Generic;
using Unleash.Strategies;
using Xunit;

namespace Unleash.Core.Tests.Strategies
{
    public class UserWithIdStrategyTest : BaseStrategyTests<UserWithIdStrategy>
    {
        protected override UserWithIdStrategy CreateStrategy() => new UserWithIdStrategy();

        [Fact]
        public void IsEnabled_WhenPassedEmptyParameters_ShouldReturnFalse()
        {
            var emptyParameters = new Dictionary<string, string>();
            var context = new UnleashContext.Builder().Build();
            var result = Strategy.IsEnabled(emptyParameters, context);
            Assert.False(result);
        }

        [Theory]
        [InlineData(null, "123", false)]
        [InlineData("", "123", false)]
        [InlineData("123", "", false)]
        [InlineData("", "", false)]
        [InlineData("123", "123", true)]
        [InlineData("123", "123, 122, 121", true)]
        [InlineData("123", "123,122,121", true)]
        [InlineData("123", "121,123,122", true)]
        [InlineData("123", "12345", false)]
        [InlineData("12", "121,122,123,1212", false)]
        [InlineData("12", "121, 122, 123, 1212", false)]
        public void IsEnabled_WhenContextIsComparedToParameters_ShouldReturnExpectedResult(
            string contextUserId,
            string userIdsParameterValue,
            bool expectedResult)
        {
            var contextBuilder = UnleashContext.New();

            if (contextUserId != null)
            {
                contextBuilder = contextBuilder.UserId(contextUserId);
            }

            var context = contextBuilder.Build();

            var parameters = new Dictionary<string, string>
            {
                [Strategy.UserIdsConst] = userIdsParameterValue
            };

            var actualResult = Strategy.IsEnabled(parameters, context);

            Assert.Equal(expectedResult, actualResult);
        }
    }
}
