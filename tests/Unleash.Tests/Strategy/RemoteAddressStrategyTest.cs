using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Unleash.Strategies;

namespace Unleash.Tests.Strategy
{
    public class RemoteAddressStrategyTest
    {
        private readonly RemoteAddressStrategy strategy;

        public RemoteAddressStrategyTest()
        {
            strategy = new RemoteAddressStrategy();
        }

        private static string FIRST_IP = "127.0.0.1";
        private static string SECOND_IP = "10.0.0.1";
        private static string THIRD_IP = "196.0.0.1";

        private static List<string> ALL = new List<string>()
        {
            FIRST_IP,
            SECOND_IP,
            THIRD_IP
        };

        public static object[] data =
        {
            new object[] {FIRST_IP, FIRST_IP, true},
            new object[] {FIRST_IP, string.Join(",", ALL), true},
            new object[] {SECOND_IP, string.Join(",", ALL), true},
            new object[] {THIRD_IP, string.Join(",", ALL), true},
            new object[] {FIRST_IP, string.Join(", ", ALL), true},
            new object[] {SECOND_IP, string.Join(", ", ALL), true},
            new object[] {THIRD_IP, string.Join(", ", ALL), true},
            new object[] {SECOND_IP, string.Join(",  ", ALL), true},
            new object[] {SECOND_IP, string.Join(".", ALL), false},
            new object[] {FIRST_IP, SECOND_IP, false},
        };

        [Test]
        public void should_have_a_name()
        {
            strategy.Name.Should().Be("remoteAddress");
        }

        [Test, TestCaseSource("data")]
        public void test(string actualIp, string parameterstring, bool expected)
        {

            var context = UnleashContext.New().RemoteAddress(actualIp).Build();
            var parameters = setupParameterMap(parameterstring);

            strategy.IsEnabled(parameters, context).Should().Be(expected);
        }

        private Dictionary<string, string> setupParameterMap(string ipstring)
        {
            var parameters = new Dictionary<string, string>();
            parameters.Add(RemoteAddressStrategy.PARAM, ipstring);
            return parameters;
        }

        [Test]
        public void FindsInCIDRRange()
        {
            var range = "73.125.227.0/29";
            var input = "73.125.227.7";
            var context = UnleashContext.New().RemoteAddress(input).Build();
            var parameters = setupParameterMap(range);
            strategy.IsEnabled(parameters, context).Should().BeTrue();
        }

        [Test]
        public void MatchOnRangeAndSingleIP()
        {
            var range = "73.125.227.0/29,73.125.227.7";
            var input = "73.125.227.7";
            var context = UnleashContext.New().RemoteAddress(input).Build();
            var parameters = setupParameterMap(range);
            strategy.IsEnabled(parameters, context).Should().BeTrue();
        }

        [Test]
        public void MatchOnSingleIPOutsideRange()
        {
            var range = "73.125.227.0/29,73.125.227.114";
            var input = "73.125.227.114";
            var context = UnleashContext.New().RemoteAddress(input).Build();
            var parameters = setupParameterMap(range);
            strategy.IsEnabled(parameters, context).Should().BeTrue();
        }

        [Test]
        public void MatchOnRangeNotSingleIP()
        {
            var range = "73.125.227.0/29,73.125.227.114";
            var input = "73.125.227.7";
            var context = UnleashContext.New().RemoteAddress(input).Build();
            var parameters = setupParameterMap(range);
            strategy.IsEnabled(parameters, context).Should().BeTrue();
        }

        [Test]
        public void DoesntMatchOnCIDRJunk()
        {
            var range = "73.125.227.0/junk";
            var input = "73.125.227.7";
            var context = UnleashContext.New().RemoteAddress(input).Build();
            var parameters = setupParameterMap(range);
            strategy.IsEnabled(parameters, context).Should().BeFalse();
        }

        [Test]
        public void GuardsAgainstTooHighCIDR()
        {
            var range = "73.125.227.1/59";
            var input = "73.125.227.1";
            var context = UnleashContext.New().RemoteAddress(input).Build();
            var parameters = setupParameterMap(range);
            strategy.IsEnabled(parameters, context).Should().BeTrue();
        }

        [Test]
        public void RepeatingAndReusingTheCheckDoesntBreakThings()
        {
            var range = "73.125.227.0/29";
            var correctInput = "73.125.227.7";
            var wrongInput = "73.125.227.9";
            var contextWithCorrect = UnleashContext.New().RemoteAddress(correctInput).Build();
            var contextWithWrong = UnleashContext.New().RemoteAddress(wrongInput).Build();
            var parameters = setupParameterMap(range);
            strategy.IsEnabled(parameters, contextWithCorrect).Should().BeTrue();
            strategy.IsEnabled(parameters, contextWithWrong).Should().BeFalse();
            strategy.IsEnabled(parameters, contextWithCorrect).Should().BeTrue();
        }
    }
}