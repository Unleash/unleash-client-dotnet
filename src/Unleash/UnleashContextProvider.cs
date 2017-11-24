namespace Unleash
{
    public interface IUnleashContextProvider
    {
        UnleashContext Context { get; }
    }

    internal class DefaultUnleashContextProvider : IUnleashContextProvider
    {
        private readonly UnleashContext context;

        public DefaultUnleashContextProvider(UnleashContext context = null)
        {
            this.context = context;
        }

        public UnleashContext Context => context ?? UnleashContext.New().Build();
    }
}