using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Unleash.Tests.Mock
{
    internal class HttpClientFactoryMock : DefaultHttpClientFactory
    {
        public bool CreateHttpClientInstanceCalled { get; private set; }

        protected override HttpClient CreateHttpClientInstance(Uri unleashApiUri)
        {
            CreateHttpClientInstanceCalled = true;

            return base.CreateHttpClientInstance(unleashApiUri);
        }

    }
}
