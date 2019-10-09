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
    }
}