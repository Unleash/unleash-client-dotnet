using System;
using System.IO;
using System.Text;

namespace Unleash.Serialization
{
    internal class DynamicNewtonsoftJsonSerializer : IDynamicJsonSerializer
    {
        public string NugetPackageName => "Newtonsoft.Json (>= 9.0.1)";

        private readonly Encoding encoding = Encoding.UTF8;

        private Type jsonTextWriterType;
        private Type jsonTextReaderType;

        private dynamic serializer;

        public bool TryLoad()
        {
            var jsonSerializerType = Type.GetType("Newtonsoft.Json.JsonSerializer, Newtonsoft.Json");
            if (jsonSerializerType == null)
                return false;

            serializer = Activator.CreateInstance(jsonSerializerType);

            var namingStrategyType = Type.GetType("Newtonsoft.Json.Serialization.CamelCaseNamingStrategy, Newtonsoft.Json");
            if (namingStrategyType == null)
                return false;

            dynamic namingStrategy = Activator.CreateInstance(namingStrategyType);
            namingStrategy.ProcessDictionaryKeys = false;

            var contractResolverType = Type.GetType("Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver, Newtonsoft.Json");
            dynamic contractResolver = Activator.CreateInstance(contractResolverType);
            contractResolver.NamingStrategy = namingStrategy;

            serializer.ContractResolver = contractResolver;

            jsonTextReaderType = Type.GetType("Newtonsoft.Json.JsonTextReader, Newtonsoft.Json");
            jsonTextWriterType = Type.GetType("Newtonsoft.Json.JsonTextWriter, Newtonsoft.Json");

            return true;
        }

        public T Deserialize<T>(Stream stream)
        {
            using (var streamReader = new StreamReader(stream, encoding))
            {
                dynamic textReader = Activator.CreateInstance(jsonTextReaderType, streamReader);

                try
                {
                    return serializer.Deserialize<T>(textReader);
                }
                finally
                {
                    (textReader as IDisposable)?.Dispose();
                }
            }
        }

        public void Serialize<T>(Stream stream, T instance)
        {
            // Default
            const int bufferSize = 1024 * 4;

            // Client code needs to dispose this.
            const bool leaveOpen = true;

            using (var writer = new StreamWriter(stream, encoding, bufferSize, leaveOpen: leaveOpen))
            {
                dynamic jsonWriter = Activator.CreateInstance(jsonTextWriterType, writer);

                try
                {
                    serializer.Serialize(jsonWriter, instance);

                    jsonWriter.Flush();
                    stream.Position = 0;
                }
                finally
                {
                    (jsonWriter as IDisposable)?.Dispose();
                }
            }
        }
    }
}