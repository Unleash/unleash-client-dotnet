using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Unleash.Strategies;
using Unleash.Internal;

namespace Unleash.Tests.Strategy
{
    public class ApplicationHostnameStrategyTest
    {
        [TearDown]
        public void Remove_hostname_property()
        {
            Environment.SetEnvironmentVariable("hostname", null);
        }

        [Test]
        public void should_be_disabled_if_no_HostNames_in_parameters()
        {
            var strategy = new ApplicationHostnameStrategy();
            var parameters = new Dictionary<string, string>();
            parameters.Add("hostNames", null);

            strategy.IsEnabled(parameters).Should().BeFalse();
        }

        [Test]
        public void should_be_disabled_if_hostname_not_in_list()
        {
            var strategy = new ApplicationHostnameStrategy();

            var parameters = new Dictionary<string, string>();
            parameters.Add("hostNames", "MegaHost,MiniHost, happyHost");

            strategy.IsEnabled(parameters).Should().BeFalse();
        }

        [Test]
        public void should_be_enabled_for_hostName()
        {
            string hostName = "my-super-host";
            Environment.SetEnvironmentVariable("hostname", hostName);

            var strategy = new ApplicationHostnameStrategy();

            var parameters = new Dictionary<string, string>();
            parameters.Add("hostNames", "MegaHost," + hostName + ",MiniHost, happyHost");

            strategy.IsEnabled(parameters).Should().BeTrue();
        }

        [Test]
        public void should_handle_weird_casing()
        {
            string hostName = "my-super-host";
            Environment.SetEnvironmentVariable("hostname", hostName);

            var strategy = new ApplicationHostnameStrategy();

            var parameters = new Dictionary<string, string>();
            parameters.Add("hostNames", $"MegaHost,{hostName.ToUpperInvariant()},MiniHost, happyHost");

            strategy.IsEnabled(parameters).Should().BeTrue();
        }

        [Test]
        public void so_close_but_no_cigar()
        {
            string hostName = "my-super-host";
            Environment.SetEnvironmentVariable("hostname", hostName);

            var strategy = new ApplicationHostnameStrategy();

            var parameters = new Dictionary<string, string>();

            parameters.Add("hostNames", "MegaHost, MiniHost, SuperhostOne");
            strategy.IsEnabled(parameters).Should().BeFalse();
        }

        [Test]
        public void should_be_enabled_for_InetAddress()
        {
            var hostName = UnleashExtensions.GetLocalIpAddress(); 
            Environment.SetEnvironmentVariable("hostname", hostName);

            var strategy = new ApplicationHostnameStrategy();

            var parameters = new Dictionary<string, string>();
            parameters.Add("hostNames", $"MegaHost,{hostName},MiniHost, happyHost");
            strategy.IsEnabled(parameters).Should().BeTrue();
        }

        [Test]
        public void should_be_enabled_for_dashed_host()
        {
            var hostName = "super-wiEred-host";
            Environment.SetEnvironmentVariable("hostname", hostName);

            var strategy = new ApplicationHostnameStrategy();

            var parameters = new Dictionary<string, string>();
            parameters.Add("hostNames", $"MegaHost,{hostName},MiniHost, happyHost");

            strategy.IsEnabled(parameters).Should().BeTrue();
        }

        [Test]
        public void null_test()
        {
            var strategy = new ApplicationHostnameStrategy();
            strategy.IsEnabled(new Dictionary<string, string>()).Should().BeFalse();
        }
    }
}