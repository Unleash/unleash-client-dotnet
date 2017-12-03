namespace Unleash.Strategies
{
    using System;

    internal class StrategyUtils
    {
        /// <summary>
        /// Takes to string inputs concat them, produce a hashCode and return a normalized value between 0 and 100;
        /// </summary>
        public static int GetNormalizedNumber(string identifier, string groupId)
        {
            const int one = 1;
            const int oneHundred = 100;
            const string separator = ":";

            var hashCode = Math.Abs(string.Concat(groupId, separator, identifier)
                .GetHashCode());

            return hashCode % oneHundred + one;
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
    }
}