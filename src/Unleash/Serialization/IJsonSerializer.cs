using System.IO;

namespace Unleash.Serialization
{
    public interface IJsonSerializer
    {
        T Deserialize<T>(Stream stream);
        Stream Serialize<T>(T instance);
    }
}