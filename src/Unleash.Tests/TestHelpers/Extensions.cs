using Unleash.Serialization;

namespace Unleash.Tests
{
    public static class Extensions
    {
        public static string SerializeObjectToString(this IJsonSerializer serializer, object o)
        {
            using (var stream = serializer.Serialize(o))
            {
                return stream.ConvertToString();
            }
        }
    }
}