using System;
using System.Collections.Generic;
using System.Text;

namespace Unleash.Internal
{
    public class Override
    {
        public Override(string contextName, string[] values)
        {
            ContextName = contextName;
            Values = values;
        }

        public string ContextName { get; }
        public string[] Values { get; }
    }
}
