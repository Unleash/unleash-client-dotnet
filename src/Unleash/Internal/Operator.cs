using System;
using System.Collections.Generic;
using System.Text;

namespace Unleash.Internal
{
    // Compile time checking of constant strings.
    // Allows for invalid operators without crashing the application
    public class Operator
    {
        public const string IN = nameof(IN);
        public const string NOT_IN = nameof(NOT_IN);
        public const string STR_ENDS_WITH = nameof(STR_ENDS_WITH);
        public const string STR_STARTS_WITH = nameof(STR_STARTS_WITH);
        public const string STR_CONTAINS = nameof(STR_CONTAINS);
        public const string NUM_EQ = nameof(NUM_EQ);
        public const string NUM_GT = nameof(NUM_GT);
        public const string NUM_GTE = nameof(NUM_GTE);
        public const string NUM_LT = nameof(NUM_LT);
        public const string NUM_LTE = nameof(NUM_LTE);
        public const string DATE_AFTER = nameof(DATE_AFTER);
        public const string DATE_BEFORE = nameof(DATE_BEFORE);
        public const string SEMVER_EQ = nameof(SEMVER_EQ);
        public const string SEMVER_GT = nameof(SEMVER_GT);
        public const string SEMVER_LT = nameof(SEMVER_LT);
    }
}
