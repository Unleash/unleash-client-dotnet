﻿using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Unleash.Scheduling;
using Unleash.Tests.Mock;
using static Unleash.Tests.Specifications.TestFactory;

namespace Unleash.Tests
{
    public class DefaultUnleashTests
    {
        [Test]
        public void ConfigureEvents_should_invoke_callback()
        {
            // Arrange
            var settings = new UnleashSettings
            {
                AppName = "testapp",
                DisableSingletonWarning = true
            };

            var unleash = new DefaultUnleash(settings);
            var callbackCalled = false;

            // Act
            unleash.ConfigureEvents(cfg =>
            {
                callbackCalled = true;
            });

            // Assert
            callbackCalled.Should().BeTrue();
        }

        [Test]
        public void Configure_Http_Client_Factory()
        {
            // Arrange
            var factory = new HttpClientFactoryMock();
            var apiUri = new Uri("http://localhost:8080/");

            // Act
            var client = factory.Create(apiUri);

            // Assert
            factory.CreateHttpClientInstanceCalled.Should().BeTrue();
        }

        public static IUnleash CreateUnleash(string name, string state)
        {
            var fakeHttpClientFactory = A.Fake<IHttpClientFactory>();
            var fakeHttpMessageHandler = new TestHttpMessageHandler();
            var httpClient = new HttpClient(fakeHttpMessageHandler) { BaseAddress = new Uri("http://localhost") };
            var fakeScheduler = A.Fake<IUnleashScheduledTaskManager>();
            var fakeFileSystem = new MockFileSystem();
            var toggleState = Newtonsoft.Json.JsonConvert.SerializeObject(state, new Newtonsoft.Json.JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver
                {
                    NamingStrategy = new Newtonsoft.Json.Serialization.CamelCaseNamingStrategy()
                }
            });

            A.CallTo(() => fakeHttpClientFactory.Create(A<Uri>._)).Returns(httpClient);
            A.CallTo(() => fakeScheduler.Configure(A<IEnumerable<IUnleashScheduledTask>>._, A<CancellationToken>._)).Invokes(action =>
            {
                var task = ((IEnumerable<IUnleashScheduledTask>)action.Arguments[0]).First();
                task.ExecuteAsync((CancellationToken)action.Arguments[1]).Wait();
            });

            fakeHttpMessageHandler.Response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(state, Encoding.UTF8, "application/json"),
                Headers =
                {
                    ETag = new EntityTagHeaderValue("\"123\"")
                }
            };

            var settings = new UnleashSettings
            {
                AppName = name,
                HttpClientFactory = fakeHttpClientFactory,
                ScheduledTaskManager = fakeScheduler,
                FileSystem = fakeFileSystem,
                DisableSingletonWarning = true
            };

            var unleash = new DefaultUnleash(settings);

            return unleash;
        }
    }
}
