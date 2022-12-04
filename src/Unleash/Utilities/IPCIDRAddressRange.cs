using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Unleash.Utilities
{
    internal class IPCIDRAddressRange
    {
        private int cidrCount;
        private IPAddress ipAddress;
        private byte[] baseIPBytes;

        public IPCIDRAddressRange(string address)
        {
            var ipAndCidrPair = address.Split('/');
            cidrCount = int.Parse(ipAndCidrPair[1]);
            ipAddress = IPAddress.Parse(ipAndCidrPair[0]);
            baseIPBytes = ipAddress.GetAddressBytes();
        }

        public bool Contains(IPAddress remoteAddress)
        {
            var remoteBytes = remoteAddress.GetAddressBytes();

            if (remoteBytes.Length != baseIPBytes.Length)
                return false;

            var remaining = cidrCount;
            var currentByte = 0;

            // Compare all bytes fully part of the subnet mask
            while (remaining > 8)
            {
                if (remoteBytes[currentByte] != baseIPBytes[currentByte])
                    return false;

                remaining -= 8;
                currentByte++;
            }

            // We've reached the end of the CIDR subnet mask
            if (remaining == 0)
                return true;

            // Blank out all variable bits so we can compare the bytes to each other
            for (var shift = 0; shift + remaining < 8; shift++)
            {
                byte mask = (byte)(1 << shift);
                remoteBytes[currentByte] &= (byte)~mask;
                baseIPBytes[currentByte] &= (byte)~mask;
            }

            // Done blanking out, compare
            return remoteBytes[currentByte] == baseIPBytes[currentByte];
        }
    }
}
