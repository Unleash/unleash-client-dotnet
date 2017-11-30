using System;
using System.IO;
using Newtonsoft.Json;
using Unleash.Internal;
using Unleash.Serialization;

namespace Unleash.Tests
{
    public static class Extensions
    {
        public static string SerializeObjectToString(this IJsonSerializer serializer, object o)
        {
            using (var ms = new MemoryStream())
            {
                serializer.Serialize(ms, o);
                return ms.ConvertToString();
            }
        }

        public static void TraceToJson(this object o)
        {
            Console.WriteLine(JsonConvert.SerializeObject(o, Formatting.Indented));
        }
    }
}