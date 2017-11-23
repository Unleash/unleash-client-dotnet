namespace Unleash
{
    public interface IUnleashContextProvider
    {
        UnleashContext Context { get; }
    }

    internal class DefaultUnleashContextProvider : IUnleashContextProvider
    {
        public UnleashContext Context
        {
            get
            {
                var context = UnleashContext.New().Build();
                return context;
            }
        }
    }
}