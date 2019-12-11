using Unleash.Serialization;

namespace Unleash.Core.Tests.Serialization
{
    public class NewtonsoftJsonSerializerTest : BaseJsonSerializerTests<NewtonsoftJsonSerializer>
    {
        public override NewtonsoftJsonSerializer CreateSerializer()
        {
            var settings = new NewtonsoftJsonSerializerSettings();
            return new NewtonsoftJsonSerializer(settings);
        }
    }
}