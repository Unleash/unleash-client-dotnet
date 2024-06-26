namespace Unleash.Communication
{
    internal class FetchTogglesResult
    {
        public bool HasChanged { get; set; }
        public string Etag { get; set; }
        public string State { get; set; }
    }
}