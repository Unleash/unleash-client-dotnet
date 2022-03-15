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
        public string Value { get; internal set; }
        public bool CaseInsensitive { get; private set; }

        public Constraint(string contextName, Operator @operator, bool caseInsensitive, string value, params string[] values)
        {
            ContextName = contextName;
            Operator = @operator;
            CaseInsensitive = caseInsensitive;
            Values = values;
            Value = value;
        }
    }
}
