using Unleash.Strategies;
using Xunit;

namespace Unleash.Core.Tests.Strategies
{
    public abstract class BaseStrategyTests<TStrategy>
        where TStrategy : class, IStrategy
    {
        protected abstract TStrategy CreateStrategy();

        protected TStrategy Strategy => CreateStrategy();

        [Fact]
        public void Strategy_ShouldHaveName()
        {
            Assert.NotNull(Strategy.Name);
            Assert.NotEmpty(Strategy.Name);
        }
    }
}
