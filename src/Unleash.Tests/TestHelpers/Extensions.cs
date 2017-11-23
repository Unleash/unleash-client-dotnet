using System.IO;
using System.Text;

namespace Unleash.Tests.TestHelpers
{
    public static class Extensions
    {
        public static string ConvertToString(this Stream stream)
        {
            stream.Position = 0;

            using (stream)
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
}