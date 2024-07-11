using System.Text;

namespace Unleash.Tests.Mock
{
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        public List<HttpRequestMessage> SentMessages = new List<HttpRequestMessage>();

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            SentMessages.Add(request);

            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            });
        }
    }
}
