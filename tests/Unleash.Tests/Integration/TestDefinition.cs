using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Unleash.Tests.Specifications
{
    public class TestDefinition
    {
        public string Name { get; set; }
        public JObject State { get; set; }
        public List<TestCase> Tests { get; set; }
        public List<TestCaseVariant> VariantTests { get; set; }
    }
}
