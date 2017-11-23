using FluentAssertions;
using NUnit.Framework;
using Unleash.Util;

namespace Unleash.Tests
{
    public class ExampleTests
    {
        private readonly global::Unleash.IUnleash unleash;

        public ExampleTests()
        {
            var config = new UnleashConfig()
                .SetAppName("dotnet-test")
                .SetInstanceId("instance 1")
                .SetUnleashApi("http://unleash.herokuapp.com/");

            unleash = new DefaultUnleash(config);
        }

        [Test]
        public void A()
        {
            unleash.IsEnabled("abc")
                .Should().BeTrue();
        }
    }
}