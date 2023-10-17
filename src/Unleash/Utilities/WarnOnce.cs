using System;
using System.Collections.Generic;
using Unleash.Logging;

namespace Unleash.Utilities
{
    internal class WarnOnce
    {
        private readonly ILog logger;
        private readonly HashSet<string> seen = new HashSet<string>();

        public WarnOnce(ILog logger)
        {
            this.logger = logger;
        }

        public void Warn(string key, string message)
        {
            if (seen.Contains(key))
            {
                return;
            }
            
            seen.Add(key);
            logger.Warn(message);
        }
    }
}

