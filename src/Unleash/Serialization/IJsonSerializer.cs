using System.IO;

namespace Unleash.Serialization
{
    public interface IJsonSerializer
    {
        T Deserialize<T>(Stream stream);
        void Serialize<T>(Stream stream, T instance);
    }
}