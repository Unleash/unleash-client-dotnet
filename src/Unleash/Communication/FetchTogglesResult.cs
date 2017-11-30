using Unleash.Internal;

namespace Unleash.Communication
{
    internal class FetchTogglesResult
    {
        public ToggleCollection ToggleCollection { get; set; }
        public bool HasChanged { get; set; }
        public string Etag { get; set; }
    }
}