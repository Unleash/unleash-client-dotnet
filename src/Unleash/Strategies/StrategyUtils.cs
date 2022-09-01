using Murmur;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Unleash.Internal;

namespace Unleash.Strategies
{
    internal class StrategyUtils
    {
        /// <summary>
        /// Takes to string inputs concat them, produce a hashCode and return a normalized value between 0 and 100;
        /// </summary>
        public static int GetNormalizedNumber(string identifier, string groupId, int normalizer = 100)
        {
            const int one = 1;
            const string separator = ":";

            byte[] bytes = Encoding.UTF8.GetBytes(string.Concat(groupId, separator, identifier));

            using (var algorithm = MurmurHash.Create32())
            {
                var hash = algorithm.ComputeHash(bytes);
                var value = BitConverter.ToUInt32(hash, 0);
                return (int)(value % normalizer + one);
            }
        }

        /// <summary>
        /// Takes a numeric string value and converts it to a int between 0 and 100.
        /// 
        /// returns 0 if the string is not numeric
        /// </summary>
        /// <returns>Return an int between 0 and 100</returns>
        /// <param name="percentage">A numeric string value</param>
        public static int GetPercentage(string percentage)
        {
            var p = int.TryParse(percentage, out var result) 
                ? result 
                : 0;

            // Ensure between 0 and 100.
            if (p > 100)
            {
                return 100;
            }
            if (p < 0)
            {
                return 0;
            }

            return p;
        }

        public static bool IsEnabled(IStrategy strategy, Dictionary<string, string> parameters, UnleashContext context, IEnumerable<Constraint> constraints)
        {
            return ConstraintUtils.Validate(constraints, context) && strategy.IsEnabled(parameters, context);
        }
    }
}