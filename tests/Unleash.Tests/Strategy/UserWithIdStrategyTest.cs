using Unleash.Strategies;

namespace Unleash.Tests.Strategy
{
    using System.Collections.Generic;
    using FluentAssertions;
    using NUnit.Framework;

    public class UserWithIdStrategyTest
    {
        private readonly UserWithIdStrategy strategy;

        public UserWithIdStrategyTest()
        {
            strategy = new UserWithIdStrategy();
        }

        [Test]
        public void should_have_expected_strategy_name()
        {
            strategy.Name.Should().Be("userWithId");
        }

        [Test]
        public void should_match_one_userId()
        {
            var parameters = new Dictionary<string, string>();

            var context = UnleashContext.New().UserId("123").Build();
            parameters.Add(strategy.UserIdsConst, "123");

            strategy.IsEnabled(parameters, context)
				.Should().BeTrue();
        }

        [Test]
        public void should_match_first_userId_in_list()
        {
            var parameters = new Dictionary<string, string>();

            var context = UnleashContext.New().UserId("123").Build();
            parameters.Add(strategy.UserIdsConst, "123, 122, 121");

            strategy.IsEnabled(parameters, context).Should().BeTrue();
        }

        [Test]
        public void should_match_middle_userId_in_list()
        {
            var parameters = new Dictionary<string, string>();

            var context = UnleashContext.New().UserId("122").Build();
            parameters.Add(strategy.UserIdsConst, "123, 122, 121");

            strategy.IsEnabled(parameters, context).Should().BeTrue();
        }

        [Test]
        public void should_match_last_userId_in_list()
        {
            var parameters = new Dictionary<string, string>();
            parameters.Add(strategy.UserIdsConst, "123, 122, 121");

            var context = UnleashContext.New().UserId("121").Build();

            strategy.IsEnabled(parameters, context).Should().BeTrue();
        }

        [Test]
        public void should_not_match_subparts_of_ids()
        {
            var parameters = new Dictionary<string, string>();

            var context = UnleashContext.New().UserId("12").Build();
            parameters.Add(strategy.UserIdsConst, "123, 122, 121, 212");

            strategy.IsEnabled(parameters, context).Should().BeFalse();
        }

        [Test]
        public void should_match_csv_without_space()
        {
            var parameters = new Dictionary<string, string>();

            var context = UnleashContext.New().UserId("123").Build();
            parameters.Add(strategy.UserIdsConst, "123,122,121");

            strategy.IsEnabled(parameters, context).Should().BeTrue(); ;
        }

        [Test]
        public void should_match_real_ids()
        {
            var parameters = new Dictionary<string, string>();

            var context = UnleashContext.New().UserId("298261117").Build();
            parameters.Add(strategy.UserIdsConst,
                "160118738, 1823311338, 1422637466, 2125981185, 298261117, 1829486714, 463568019, 271166598");

            strategy.IsEnabled(parameters, context).Should().BeTrue();
        }

        [Test]
        public void should_not_match_real_ids()
        {
            var parameters = new Dictionary<string, string>();

            var context = UnleashContext.New().UserId("32667774").Build();
            parameters.Add(strategy.UserIdsConst,
                "160118738, 1823311338, 1422637466, 2125981185, 298261117, 1829486714, 463568019, 271166598");

            strategy.IsEnabled(parameters, context).Should().BeFalse();
        }

        [Test]
        public void should_not_be_enabled_without_id()
        {
            var parameters = new Dictionary<string, string>();
            parameters.Add(strategy.UserIdsConst, "160118738, 1823311338");

            strategy.IsEnabled(parameters).Should().BeFalse();
        }
    }
}