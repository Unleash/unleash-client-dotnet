namespace Unleash.Communication.Admin.Dto
{
    public class State
    {
        public int Version { get; set; }
        public FeatureToggle[] Features { get; set; }
        public Strategy[] Strategies { get; set; }
    }
}