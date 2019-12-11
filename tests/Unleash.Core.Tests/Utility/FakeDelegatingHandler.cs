using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Unleash.Core.Tests.Utility;

namespace Unleash.Core.Tests.Communication
{
    public class FakeDelegatingHandler : DelegatingHandler
    {
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
