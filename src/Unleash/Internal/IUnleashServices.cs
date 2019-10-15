using System;
using System.Collections.Generic;
using Unleash.Strategies;

namespace Unleash.Internal
{
    public interface IUnleashServices : IDisposable
    {
        IReadOnlyDictionary<string, IStrategy> StrategyMap { get; }

        ToggleCollection GetToggleCollection();
        void RegisterCount(string toggleName, bool enabled);
    }
}
