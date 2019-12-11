using System;
using System.Collections.Generic;
using Unleash.Internal;
using Unleash.Strategies;
using Xunit;

namespace Unleash.Core.Tests.Strategies
{
    public class ApplicationHostnameStrategyTests : BaseStrategyTests<ApplicationHostnameStrategy>, IDisposable
    {
        private readonly string _hostName = Environment.GetEnvironmentVariable("hostname");

        protected override ApplicationHostnameStrategy CreateStrategy() => new ApplicationHostnameStrategy();

        public static IEnumerable<object[]> Data
        {
            get
            {
                yield return new object[]
                {
                    null,
                    new Func<string, string>(hostName => string.Empty),
                    true
                };
                yield return new object[]
                {
                    "my-super-host",
                    new Func<string, string>(hostName => $"MegaHost,{hostName.ToUpperInvariant()},MiniHost, happyHost"),
                    true
                };
            }
        }

        [Fact]
        public void IsEnabled_WhenPassedEmptyParameters_ShouldReturnFalse()
        {
            var emptyParameters = new Dictionary<string, string>();
            var context = new UnleashContext.Builder().Build();
            var result = Strategy.IsEnabled(emptyParameters, context);
            Assert.False(result);
        }

        [Theory]
        [InlineData(null, "some-host", false)]
        [InlineData("", "some-host", false)]
        [InlineData("my-host", "", false)]
        [InlineData("my-host", "not-my-host,also-not-my-host,and-not-my-host-either", false)]
        [InlineData("my-host", "other-host-1,other-host-2", false)]
        [InlineData("", "", false)]
        [InlineData("my-host", "my-host", true)]
        [InlineData("my-host", "your-host,my-host,his-host,her-host,their-host", true)]
        [InlineData("my-host", "your-host, my-host, his-host, her-host, their-host", true)]
        [InlineData("my-host", "your-host,My-Host,his-host,her-host,their-host", true)]
        [InlineData("my-host", "your-host, My-Host, his-host, her-host, their-host", true)]
        [InlineData("my-host", "your-host,MY-HOST,his-host,her-host,their-host", true)]
        [InlineData("my-host", "your-host, MY-HOST, his-host, her-host, their-host", true)]
        [InlineData("MY-HOST", "your-host,my-host,his-host,her-host,their-host", true)]
        [InlineData("MY-HOST", "your-host, my-host, his-host, her-host, their-host", true)]
        public void IsEnabled_WhenContextIsComparedToParameters_ShouldReturnExpectedResult(
            string hostName,
            string hostNamesParameterValueFormat,
            bool expectedResult)
        {
            if (hostName != null)
            {
                Environment.SetEnvironmentVariable("hostname", hostName);
            }

            var context = new UnleashContext.Builder().Build();
            var parameters = new Dictionary<string, string>
            {
                ["hostNames"] = string.Format(hostNamesParameterValueFormat, hostName)
            };

            var actualResult = Strategy.IsEnabled(parameters, context);
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void IsEnabled_WhenHostNameIsIpv4Address_ShouldReturnTrue()
        {
            var hostName = UnleashExtensions.GetLocalIpAddress();
            Environment.SetEnvironmentVariable("hostname", hostName);

            var context = new UnleashContext.Builder().Build();
            var parameters = new Dictionary<string, string>
            {
                ["hostNames"] = $"MegaHost,{hostName},MiniHost, happyHost"
            };

            var isEnabled = Strategy.IsEnabled(parameters, context);
            Assert.True(isEnabled);
        }

        [Fact]
        public void IsEnabled_WhenHostNameIsNotSet_ShouldReturnFalse()
        {
            var context = new UnleashContext.Builder().Build();
            var parameters = new Dictionary<string, string>();

            var isEnabled = Strategy.IsEnabled(parameters, context);
            Assert.False(isEnabled);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Environment.SetEnvironmentVariable("hostname", _hostName);
        }
    }
}
