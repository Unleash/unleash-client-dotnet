using System;
using System.Collections.Generic;
using System.Text;

namespace Unleash.Internal
{
    public class Segment
    {
        public string Id { get; }
        public List<Constraint> Constraints { get; }
        public Segment(string id, List<Constraint> constraints = null)
        {
            Id = id;
            Constraints = constraints ?? new List<Constraint>();
        }
    }
}
