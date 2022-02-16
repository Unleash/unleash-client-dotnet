using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Unleash.Internal;
using Unleash.Serialization;
using Unleash.Tests.Mock;
using Unleash.Tests.Serialization;
using Unleash.Utilities;

namespace Unleash.Tests.Utilities
{
    public class ToggleBootstrapUrlProvider_Tests
    {
        [Test]
        public void Gets_The_File_Content()
        {
            // Arrange
            var path = "http://localhost/path/to/file";
            var content = "{}";
            var messageHandlerMock = new ConfigurableMessageHandlerMock();
            var returnMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            };
            messageHandlerMock.Configure(path, returnMessage);
            var client = new HttpClient(messageHandlerMock);
            var bootstrapUrlProvider = new ToggleBootstrapUrlProvider(path, client, new JsonNetSerializer());

            // Act
            var responseContent = bootstrapUrlProvider.Read();

            // Assert
            responseContent.Features.Should().BeEmpty();
            messageHandlerMock.SentMessages.First().Method.Should().Be(HttpMethod.Get);
            messageHandlerMock.SentMessages.First().RequestUri.ToString().Should().Be(path);
        }

        [Test]
        public void Returns_Empty_When_The_Result_Fails()
        {
            // Arrange
            var path = "http://localhost/path/to/file";
            var messageHandlerMock = new ConfigurableMessageHandlerMock();
            var returnMessage = new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
            messageHandlerMock.Configure(path, returnMessage);
            var client = new HttpClient(messageHandlerMock);
            var bootstrapUrlProvider = new ToggleBootstrapUrlProvider(path, client, null);

            // Act
            var responseContent = bootstrapUrlProvider.Read();

            // Assert
            responseContent.Should().Be(null);
            messageHandlerMock.SentMessages.First().Method.Should().Be(HttpMethod.Get);
            messageHandlerMock.SentMessages.First().RequestUri.ToString().Should().Be(path);
        }

        [Test]
        public void Throws_When_Request_Fails_And_Is_Configured_To_Throw()
        {
            // Arrange
            var path = "http://localhost/path/to/file";
            var messageHandlerMock = new ConfigurableMessageHandlerMock();
            var returnMessage = new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
            messageHandlerMock.Configure(path, returnMessage);
            var client = new HttpClient(messageHandlerMock);
            var bootstrapUrlProvider = new ToggleBootstrapUrlProvider(path, client, null, true);

            // Act, Assert
            Assert.Throws<FetchingToggleBootstrapUrlFailedException>(() => { var responseContent = bootstrapUrlProvider.Read(); });
        }

        [Test]
        public void Request_Includes_Custom_Headers()
        {
            // Arrange
            var path = "http://localhost/path/to/file";
            var customHeaders = new Dictionary<string, string>()
            {
                { "Authorization", "Bearer longtokenstring" }
            };
            var content = "{}";
            var messageHandlerMock = new ConfigurableMessageHandlerMock();
            var returnMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            };
            messageHandlerMock.Configure(path, returnMessage);
            var client = new HttpClient(messageHandlerMock);
            var bootstrapUrlProvider = new ToggleBootstrapUrlProvider(path, client, new JsonNetSerializer(), false, customHeaders);

            // Act
            var responseContent = bootstrapUrlProvider.Read();

            // Assert
            var configuredHeader = customHeaders.First();
            responseContent.Features.Should().BeEmpty();
            messageHandlerMock.SentMessages.First().Method.Should().Be(HttpMethod.Get);
            messageHandlerMock.SentMessages.First().RequestUri.ToString().Should().Be(path);
            messageHandlerMock.SentMessages.First().Headers.Any(kvp => kvp.Key == configuredHeader.Key && kvp.Value.First() == configuredHeader.Value).Should().BeTrue();
        }
    }
}
