using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unleash.Strategies;

namespace Unleash.Internal
{
    public interface IUnleashServices : IDisposable
    {
        IRandom Random { get; }
        IReadOnlyDictionary<string, IStrategy> StrategyMap { get; }

        Task FeatureToggleLoadComplete(bool onlyOnEmptyCache = true, CancellationToken cancellationToken = default(CancellationToken));

        ToggleCollection GetToggleCollection();
        void RegisterCount(string toggleName, bool enabled);
    }
}
