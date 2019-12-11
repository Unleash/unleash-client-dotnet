namespace Unleash.Communication.Admin.Dto
{
    public class Strategy
    {
        public string Name { get; set; }
        public string Description { get; set; }
        // public Dictionary<string, string> Parameters { get; set; }
        public StrategyParameter[] Parameters { get; set; }
    }
}