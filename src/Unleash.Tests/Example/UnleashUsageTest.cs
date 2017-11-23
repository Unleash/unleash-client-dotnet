using FluentAssertions;
using NUnit.Framework;
using Unleash.Util;

namespace Unleash.Tests.Example
{
    public class UnleashUsageTest
    {
        [Test]
        public void wire()
        {
            var config = new UnleashConfig()
                .SetAppName("test")
                .SetInstanceId("my-hostname:6517")
                .SetUnleashApi("http://localhost:4242/api");

            var unleash = new DefaultUnleash(config, new CustomStrategy());

            unleash.IsEnabled("myFeature").Should().BeFalse();
        }
    }
}