using NUnit.Framework;
using System;

namespace Unleash.Tests.Communication
{
    public class UnleashHttpClientFactory_RegisterHttpClient_Tests
    {
        private Uri _apiUri { get; } = new Uri("http://unleash.herokuapp.com/");

        private Uri _newApiUri { get; } = new Uri("http://unleash2.herokuapp2.com/");

        private DefaultHttpClientFactory _httpClientFactory { get; } = new DefaultHttpClientFactory();
        [Test]
        public void HttpClientFactory_Should_Create_Single_Instance_By_Dns_Success()
        {

            var httpClient = _httpClientFactory.Create(_apiUri);
            var expectedHashId = httpClient.GetHashCode();

            var newHttpClient = _httpClientFactory.Create(_apiUri);
            var actualHashId = newHttpClient.GetHashCode();

            Assert.AreEqual(expectedHashId, actualHashId);
        }

        [Test]
        public void HttpClientFactory_Should_Create_One_Instance_Per_Dns_Success()
        {

            var httpClient = _httpClientFactory.Create(_apiUri);
            var expectedHashId = httpClient.GetHashCode();

            var newHttpClient = _httpClientFactory.Create(_newApiUri);
            var actualHashId = newHttpClient.GetHashCode();

            Assert.AreNotEqual(expectedHashId, actualHashId);
        }
    }
}
