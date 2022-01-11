using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Unleash.Internal;
using Unleash.Tests.Mock;
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
            var bootstrapUrlProvider = new ToggleBootstrapUrlProvider(path, client);

            // Act
            var responseContent = bootstrapUrlProvider.Read();

            // Assert
            responseContent.Should().Be(content);
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
            var bootstrapUrlProvider = new ToggleBootstrapUrlProvider(path, client);

            // Act
            var responseContent = bootstrapUrlProvider.Read();

            // Assert
            responseContent.Should().Be(string.Empty);
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
            var bootstrapUrlProvider = new ToggleBootstrapUrlProvider(path, client, true);

            // Act, Assert
            Assert.Throws<AggregateException>(() => { var responseContent = bootstrapUrlProvider.Read(); });
        }
    }
}
