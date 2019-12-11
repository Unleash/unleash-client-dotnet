using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Unleash.Serialization
{
    public class NewtonsoftJsonSerializer : IJsonSerializer
    {
        private readonly NewtonsoftJsonSerializerSettings settings;
        private readonly JsonSerializer jsonSerializer;

        public NewtonsoftJsonSerializer(NewtonsoftJsonSerializerSettings settings)
        {
            this.settings = settings;
            this.jsonSerializer = JsonSerializer.Create(
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver
                    {
                        NamingStrategy = new CamelCaseNamingStrategy
                        {
                            ProcessDictionaryKeys = false
                        }
                    }
                });
        }

        /// <inheritdoc />
        public T Deserialize<T>(Stream stream)
        {
            using (var streamReader = new StreamReader(stream, settings.Encoding, false, settings.BufferSize))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                return jsonSerializer.Deserialize<T>(jsonReader);
            }
        }

        /// <inheritdoc />
        public void Serialize<T>(Stream stream, T instance)
        {
            using (var streamWriter = new StreamWriter(stream, settings.Encoding, settings.BufferSize, true))
            using (var jsonWriter = new JsonTextWriter(streamWriter))
            {
                jsonSerializer.Serialize(jsonWriter, instance);
            }
        }
    }
}
