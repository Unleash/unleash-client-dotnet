using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Unleash.Core.Tests.Utility
{
    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        /// <inheritdoc />
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Send(request));
        }

        public virtual HttpResponseMessage Send(HttpRequestMessage request)
        {
            throw new NotImplementedException($"Setup {nameof(FakeHttpMessageHandler)}.{nameof(Send)} for {request.Method} {request.RequestUri} in your test");
        }
    }
}
