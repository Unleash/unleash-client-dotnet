using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Linq;
using Unleash.Internal;
using Unleash.Scheduling;
using Unleash.Strategies;
using Unleash.Tests.Mock;
using Unleash.Variants;
using static Unleash.Tests.Specifications.TestFactory;
using Unleash.ClientFactory;

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

        [Test]
        public void IsEnabled_Flexible_Strategy_Test()
        {
            // Arrange
            var appname = "testapp";
            var state = @"
            {
                ""version"": 2,
                ""features"": [
                    {
                        ""name"": ""test_toggle"",
                        ""type"": ""experimental"",
                        ""enabled"": true,
                        ""impressionData"": false,
                        ""strategies"": [
                            {
                                ""name"": ""flexibleRollout"",
                                ""parameters"": {
                                    ""rollout"": ""100""
                                },
                                ""constraints"": []
                            }]
                    }
                ]
            }";
            var unleash = CreateUnleash(appname, state);

            // Act
            var result = unleash.IsEnabled("test_toggle");

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void IsEnabled_Gradual_Rollout_Random_Strategy_Test()
        {
            // Arrange
            var appname = "testapp";

            var state = @"
            {
                ""version"": 2,
                ""features"": [
                    {
                        ""name"": ""test_toggle"",
                        ""type"": ""experimental"",
                        ""enabled"": true,
                        ""impressionData"": false,
                        ""strategies"": [
                            {
                                ""name"": ""gradualRolloutRandom"",
                                ""parameters"": {
                                    ""percentage"": ""100""
                                },
                                ""constraints"": []
                            }]
                    }
                ]
            }";
            var unleash = CreateUnleash(appname, state);

            // Act
            var result = unleash.IsEnabled("test_toggle");

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void IsEnabled_Gradual_Rollout_UserId_Strategy_Test()
        {
            // Arrange
            var appname = "testapp";

            var state = @"
            {
                ""version"": 2,
                ""features"": [
                    {
                        ""name"": ""test_toggle"",
                        ""type"": ""experimental"",
                        ""enabled"": true,
                        ""impressionData"": false,
                        ""strategies"": [
                            {
                                ""name"": ""gradualRolloutUserId"",
                                ""parameters"": {
                                    ""percentage"": ""100"",
                                    ""groupId"": ""grp""
                                },
                                ""constraints"": []
                            }]
                    }
                ]
            }";

            var unleash = CreateUnleash(appname, state);

            // Act
            var result = unleash.IsEnabled("test_toggle");

            // Assert
            result.Should().BeFalse();
        }

        public static IUnleash CreateUnleash(string name, string state)
        {
            var fakeHttpClientFactory = A.Fake<IHttpClientFactory>();
            var content = new StringContent(state, Encoding.UTF8, "application/json");
            var fakeHttpMessageHandler = new TestHttpMessageHandler(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = content,
                Headers =
                {
                    ETag = new EntityTagHeaderValue("\"123\"")
                }
            });

            var httpClient = new HttpClient(fakeHttpMessageHandler) { BaseAddress = new Uri("http://localhost") };
            var fakeScheduler = A.Fake<IUnleashScheduledTaskManager>();

            A.CallTo(() => fakeHttpClientFactory.Create(A<Uri>._)).Returns(httpClient);
            A.CallTo(() => fakeScheduler.Configure(A<IEnumerable<IUnleashScheduledTask>>._, A<CancellationToken>._)).Invokes(action =>
            {
                var task = ((IEnumerable<IUnleashScheduledTask>)action.Arguments[0]).First();
                task.ExecuteAsync((CancellationToken)action.Arguments[1]).Wait();
            });


            var settings = new UnleashSettings
            {
                AppName = name,
                HttpClientFactory = fakeHttpClientFactory,
                ScheduledTaskManager = fakeScheduler,
                UseYggdrasil = true
            };

            var unleash = new DefaultUnleash(settings);

            return unleash;
        }
    }
}
