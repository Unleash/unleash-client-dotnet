using NUnit.Framework;

namespace Unleash.Tests.Communication
{
    public class RequestMessageHeadersTest
    {
        [Test]
        public void RequestMessageHeaders_Should_Contain_Headers()
        {
            var requestMessage = new HttpRequestMessage();
            requestMessage.Headers.TryAddWithoutValidation("header1", "value1");
            requestMessage.Headers.TryAddWithoutValidation("header1", "value2");

            Assert.That(requestMessage.Headers.Count, Is.EqualTo(1));
            Assert.That(requestMessage.Headers.First().Key, Is.EqualTo("header1"));
            Assert.That(requestMessage.Headers.First().Value.Last(), Is.EqualTo("value2"));
        }
    }
}