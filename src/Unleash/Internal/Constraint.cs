using System;
using System.Collections.Generic;
using System.Text;

namespace Unleash.Internal
{
    public class Constraint
    {
        public string ContextName { get; private set; }
        public string Operator { get; private set; }
        public string[] Values { get; private set; }
        public string Value { get; internal set; }
        public bool CaseInsensitive { get; private set; }
        public bool Inverted { get; private set; }


        public Constraint(string contextName, string @operator, bool caseInsensitive, bool inverted, string value, params string[] values)
        {
            ContextName = contextName;
            Operator = @operator?.Trim();
            CaseInsensitive = caseInsensitive;
            Values = values;
            Value = value;
            Inverted = inverted;
        }
    }
}
