using Unleash.Internal;

namespace Unleash.Communication
{
    public class FetchTogglesResult
    {
        public ToggleCollection ToggleCollection { get; set; }
        public bool HasChanged { get; set; }
        public string Etag { get; set; }
    }
}
