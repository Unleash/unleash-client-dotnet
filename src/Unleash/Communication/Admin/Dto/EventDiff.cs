namespace Unleash.Communication.Admin.Dto
{
    public class EventDiff
    {
        public string Kind { get; set; }
        public string[] Path { get; set; }
        public object Lhs { get; set; }
        public object Rhs { get; set; }
    }
}
