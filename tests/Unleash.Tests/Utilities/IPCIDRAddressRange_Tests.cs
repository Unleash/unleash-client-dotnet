using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Unleash.Utilities;

namespace Unleash.Tests.Utilities
{
    public class IPCIDRAddressRange_Tests
    {
        [Test]
        public void IPv4_Last_In_Range_Ok()
        {
            var range = "74.125.227.0/29";
            var input = "74.125.227.7";

            var rangeTester = new IPCIDRAddressRange(range);
            rangeTester.Contains(IPAddress.Parse(input)).Should().BeTrue();
        }

        [Test]
        public void IPv4_First_Octet_Missmatch_Fails()
        {
            var range = "73.125.227.0/29";
            var input = "74.125.227.7";

            var rangeTester = new IPCIDRAddressRange(range);
            rangeTester.Contains(IPAddress.Parse(input)).Should().BeFalse();
        }

        [Test]
        public void IPv4_Out_Of_Range_Not_Ok()
        {
            var range = "74.125.227.0/29";
            var input = "74.125.227.8";

            var rangeTester = new IPCIDRAddressRange(range);
            rangeTester.Contains(IPAddress.Parse(input)).Should().BeFalse();
        }

        [Test]
        public void IPv4_Mask_On_First_Two_Octets_Third_Octed_Differs_Is_Ok()
        {
            var range = "74.125.227.0/16";
            var input = "74.125.228.1";

            var rangeTester = new IPCIDRAddressRange(range);
            rangeTester.Contains(IPAddress.Parse(input)).Should().BeTrue();
        }

        [Test]
        public void IPv4_Zero_Bit_Mask_TODO()
        {
            var range = "74.125.227.0/0";
            var input = "74.125.228.1";

            var rangeTester = new IPCIDRAddressRange(range);
            rangeTester.Contains(IPAddress.Parse(input)).Should().BeTrue();
        }

        [Test]
        public void IPv4_Junk_In_Cidr_Throws()
        {
            var range = "74.125.227.0/junk";

            Action action = () => new IPCIDRAddressRange(range);
            action.Should().Throw<FormatException>();
        }

        [Test]
        public void CIDR_IPv6_Works()
        {
            var range = "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ff00/120";
            var input = "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ff60";
            var rangeTester = new IPCIDRAddressRange(range);
            rangeTester.Contains(IPAddress.Parse(input)).Should().BeTrue();
        }
    }
}
