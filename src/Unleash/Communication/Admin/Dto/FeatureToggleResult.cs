namespace Unleash.Communication.Admin.Dto
{
    public class FeatureToggleResult
    {
        public int Version { get; set; }
        public FeatureToggle[] Features { get; set; }
    }
}