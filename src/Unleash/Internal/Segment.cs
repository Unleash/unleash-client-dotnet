using System;
using System.Collections.Generic;
using System.Text;

namespace Unleash.Internal
{
    public class Segment
    {
        public int Id { get; }
        public List<Constraint> Constraints { get; }
        public Segment(int id, List<Constraint> constraints = null)
        {
            Id = id;
            Constraints = constraints ?? new List<Constraint>();
        }
    }
}
