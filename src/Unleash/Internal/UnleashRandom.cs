using System;

namespace Unleash.Internal
{
    public class UnleashRandom : IRandom
    {
        private readonly Random random;

        public UnleashRandom()
        {
            random = new Random();
        }

        public UnleashRandom(int seed)
        {
            random = new Random(seed);
        }

        public int Next() => random.Next();
        public int Next(int maxValue) => random.Next(maxValue);
    }
}
