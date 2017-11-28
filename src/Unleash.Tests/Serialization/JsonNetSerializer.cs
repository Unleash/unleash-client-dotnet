using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Unleash.Serialization;

namespace Unleash.Tests.Serialization
{
    public class JsonNetSerializer : IDynamicJsonSerializer
    {
        private readonly Encoding utf8 = Encoding.UTF8;

        private static readonly JsonSerializer Serializer = new JsonSerializer()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
            {
                NamingStrategy = new CamelCaseNamingStrategy()
                {
                    ProcessDictionaryKeys = false,
                }
            }
        };

        public T Deserialize<T>(Stream stream)
        {
            using (var streamReader = new StreamReader(stream, utf8))
            using (var textReader = new JsonTextReader(streamReader))
            {
                return Serializer.Deserialize<T>(textReader);
            }
        }

        public Stream Serialize<T>(T instance)
        {
            var memoryStream = new MemoryStream();

            using (var writer = new StreamWriter(memoryStream, utf8, 1024 * 4, leaveOpen: true))
            using (var jsonWriter = new JsonTextWriter(writer))
            {
                Serializer.Serialize(jsonWriter, instance);
                jsonWriter.Flush();
            }

            memoryStream.Position = 0;

            return memoryStream;
        }

        

        public string NugetPackageName => "Newtonsoft.Json 4 Real";

        public bool TryLoad()
        {
            return true;
        }
    }
}