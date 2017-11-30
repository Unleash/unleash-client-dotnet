using System;
using System.Collections.Concurrent;

namespace Unleash.Metrics
{
    internal class MetricsBucket
    {
        public ConcurrentDictionary<string, ToggleCount> Toggles { get; }

        public DateTimeOffset Start { get; private set; }
        public DateTimeOffset Stop { get; private set; }

        public MetricsBucket()
        {
            Start = DateTimeOffset.UtcNow;
            Toggles = new ConcurrentDictionary<string, ToggleCount>();
        }

        public void RegisterCount(string toggleName, bool active)
        {
            Toggles.AddOrUpdate(
                key: toggleName, 
                addValueFactory: name =>
                {
                    var counter = new ToggleCount();
                    counter.Register(active);
                    return counter;
                }, 
                updateValueFactory: (name, toggleCount) =>
                {
                    toggleCount.Register(active);
                    return toggleCount;
                });
        }

        public void End()
        {
            Stop = DateTimeOffset.UtcNow;
        }

        public void Clear()
        {
            Start = DateTimeOffset.UtcNow;
            Stop = DateTimeOffset.MinValue;

            foreach (var item in Toggles)
                item.Value.Clear();
        }
    }
}