using System;
using System.Collections.Generic;
using System.Text;

namespace Unleash.Variants
{
    public class VariantOverride
    {
        public VariantOverride(string contextName, params string[] values)
        {
            ContextName = contextName;
            Values = values;
        }

        public string ContextName { get; }
        public string[] Values { get; }
    }
}
