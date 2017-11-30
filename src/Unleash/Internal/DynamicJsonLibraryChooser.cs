using System;
using System.Collections.Generic;
using System.Linq;
using Unleash.Serialization;

namespace Unleash.Internal
{
    internal class DynamicJsonLibraryChooser
    {
        private static readonly List<IDynamicJsonSerializer> DynamicJsonSerializers = new List<IDynamicJsonSerializer>()
        {
            new DynamicNewtonsoftJsonSerializer(),
        };

        internal static IJsonSerializer CheckIfJsonSerializerCanBeInitialized(IJsonSerializer jsonSerializer)
        {
            // Success. Overridden by client..
            if (!(jsonSerializer is IDynamicJsonSerializer))
                return jsonSerializer;

            var serializer = jsonSerializer as IDynamicJsonSerializer;
            if (serializer.TryLoad())
                return jsonSerializer;

            // Failed to load default. Try the other ones if any..
            foreach (var dynamicJsonSerializer in DynamicJsonSerializers)
            {
                // Ignore: same as above
                if (dynamicJsonSerializer.Equals(jsonSerializer))
                    continue;

                if (!dynamicJsonSerializer.TryLoad())
                    continue;

                // Success, found a compatible json serializer
                return dynamicJsonSerializer;
            }

            // None
            var serializers = string.Join(", ", Enumerable.Select(DynamicJsonSerializers, x => x.NugetPackageName));
            throw new UnleashException($"Tried to load '{serializers}' json library(ies) but could not find any.{Environment.NewLine}Please add a reference to one of these nuget packages, or implement the '{nameof(IJsonSerializer)}' interface with your favorite json library. This needs to be wired up through the bootstrapping configuration.");
        }
    }
}