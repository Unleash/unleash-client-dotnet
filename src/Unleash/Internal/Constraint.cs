using System;
using System.Collections.Generic;
using System.Text;

namespace Unleash.Internal
{
    public class Constraint
    {
        public string ContextName { get; private set; }
        public Operator Operator { get; private set; }
        public string[] Values { get; private set; }

        public Constraint(string contextName, Operator @operator, params string[] values)
        {
            ContextName = contextName;
            Operator = @operator;
            Values = values;
        }
    }
}
