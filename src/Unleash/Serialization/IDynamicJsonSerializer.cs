namespace Unleash.Serialization
{
    internal interface IDynamicJsonSerializer : IJsonSerializer
    {
        string NugetPackageName { get; }
        bool TryLoad();
    }
}