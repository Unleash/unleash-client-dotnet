namespace Unleash.Internal
{
    public interface IRandom
    {
        int Next();
        int Next(int maxValue);
    }
}
