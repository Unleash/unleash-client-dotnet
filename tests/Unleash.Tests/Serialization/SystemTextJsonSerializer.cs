using System.Text.Json;
using Unleash.Serialization;

namespace Unleash.Tests.Serialization
{

    public class SystemTextJsonSerializer : IDynamicJsonSerializer
    {
       private readonly JsonSerializerOptions _options = new JsonSerializerOptions
       {
          PropertyNameCaseInsensitive = true,
          PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
          IncludeFields = true,
       };

        public string NugetPackageName => "System.text serializer";

        public T Deserialize<T>(Stream stream)
       {
          ArgumentNullException.ThrowIfNull(stream);

          return JsonSerializer.Deserialize<T>(stream, _options)!;
       }

       public void Serialize<T>(Stream stream, T instance)
       {
          ArgumentNullException.ThrowIfNull(stream);
          ArgumentNullException.ThrowIfNull(instance);
          JsonSerializer.Serialize(stream, instance, _options);
       }

        public bool TryLoad()
        {
            return true;
        }
    }
}