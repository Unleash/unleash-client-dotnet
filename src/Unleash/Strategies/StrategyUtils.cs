namespace Unleash.Strategies
{
    using System;

    internal class StrategyUtils
    {
        private const int OneHundred = 100;

        public static bool IsNotEmpty(string cs)
        {
            return !IsEmpty(cs);
        }

        public static bool IsEmpty(string cs)
        {
            return string.IsNullOrEmpty(cs);
        }

        public static bool IsNumeric(string cs)
        {
            if (IsEmpty(cs))
                return false;

            var sz = cs.Length;

            for (int i = 0; i < sz; i++)
            {
                if (char.IsDigit(cs[i]) == false)
                {
                    return false;
                }
            }
            return true;
        }

        /**
         * Takes to string inputs concat them, produce a hashCode and return a normalized value between 0 and 100;
         *
         * @param identifier
         * @param groupId
         * @return
         */
        public static int GetNormalizedNumber(string identifier, string groupId)
        {
            const string separator = ":";
            var hashCode = Math.Abs(string.Concat(groupId, separator, identifier).GetHashCode());
            return hashCode % OneHundred + 1;
        }

        /**
         * Takes a numeric string value and converts it to a int between 0 and 100.
         *
         * returns 0 if the string is not numeric.
         *
         * @param percentage - A numeric string value
         * @return a int between 0 and 100
         */
        public static int GetPercentage(string percentage)
        {
            if (IsNotEmpty(percentage) && IsNumeric(percentage))
            {
                int p = int.Parse(percentage);
                return p;
            }
            else
            {
                return 0;
            }
        }
    }
}