using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Unleash.Serialization
{
    public class NewtonsoftJsonSerializerSettings
    {
        [Required]
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        [Range(1024, int.MaxValue)]
        public int BufferSize { get; set; } = 65536;
    }
}
