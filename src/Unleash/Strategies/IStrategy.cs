using System;
using Yggdrasil;


namespace Unleash.Strategies
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines a strategy for enabling a feature.
    /// </summary>
    public interface IStrategy
    {
        /// <summary>
        /// Gets the strategy name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Calculates if the strategy is enabled for a given context
        /// </summary>
        bool IsEnabled(Dictionary<string, string> parameters, UnleashContext context);
    }

    internal class CustomStrategyAdapter : Yggdrasil.IStrategy
    {
        private IStrategy strategy { get; }

        public CustomStrategyAdapter(IStrategy strategy)
        {
            this.strategy = strategy;
        }

        public string Name => strategy.Name;

        public bool IsEnabled(Dictionary<string, string> parameters, Context context)
        {
            var currentTime = context.CurrentTime ?? DateTimeOffset.UtcNow;

            var unleashContext = new UnleashContext.Builder()
                                                    .AppName(context.AppName)
                                                    .CurrentTime(currentTime)
                                                    .Environment(context.Environment)
                                                    .UserId(context.UserId)
                                                    .SessionId(context.SessionId)
                                                    .RemoteAddress(context.RemoteAddress)
                                                    .Build();
            unleashContext.Properties = context.Properties;

            return strategy.IsEnabled(parameters, unleashContext);
        }
    }
}