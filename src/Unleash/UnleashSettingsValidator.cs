using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unleash.Serialization;

namespace Unleash
{
    internal class UnleashSettingsValidator
    {
        private static readonly List<IDynamicJsonSerializer> DynamicJsonSerializers = new List<IDynamicJsonSerializer>()
        {
            new DynamicNewtonsoftJsonSerializer(),
        };

        public void Validate(UnleashSettings settings)
        {
            if (settings.UnleashApi == null)
                throw new UnleashException("You are required to specify an unleash Api uri");

            if (settings.AppName == null)
                throw new UnleashException("You are required to specify an appName");

            if (settings.InstanceTag == null)
                throw new UnleashException("You are required to specify an instance id (test, production)");

            if (settings.JsonSerializer == null)
                throw new UnleashException("You are required to specify the unleash appName");

            CheckIfJsonSerializerCanBeInitialized(settings);
        }

        private static void CheckIfJsonSerializerCanBeInitialized(UnleashSettings settings)
        {
            var jsonSerializer = settings.JsonSerializer;

            // Success. Overridden by client..
            if (!(jsonSerializer is IDynamicJsonSerializer))
                return;

            var serializer = jsonSerializer as IDynamicJsonSerializer;
            if (serializer.TryLoad())
                return;

            // Failed to load default. Try the other ones if any..
            foreach (var dynamicJsonSerializer in DynamicJsonSerializers)
            {
                if (!dynamicJsonSerializer.TryLoad())
                    continue;

                // Success, found a compatible json serializer
                settings.JsonSerializer = dynamicJsonSerializer;
                return;
            }

            // None
            var serializers = string.Join(", ", DynamicJsonSerializers.Select(x => x.NugetPackageName));
            throw new UnleashException($"Tried to load '{serializers}' json library(ies) but could not find any.{Environment.NewLine}Please add a reference to one of these nuget packages, or implement the '{nameof(IJsonSerializer)}' interface with your favorite json library. This needs to be wired up through the bootstrapping configuration.");
        }
    }
}