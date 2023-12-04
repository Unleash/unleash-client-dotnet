using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static Unleash.Tests.Specifications.TestFactory;
using Unleash.Tests.Mock;
using Unleash.Internal;
using Unleash.Scheduling;
using System.Threading;
using Unleash.Variants;

namespace Unleash.Tests.Internal
{
    public class ImpressionData_Tests
    {
        [Test]
        public void Impression_Event_Gets_Called_For_IsEnabled()
        {
            // Arrange
            ImpressionEvent callbackEvent = null;
            var appname = "testapp";

            var state = @"
            {
                ""version"": 2,
                ""features"": [
                    {
                        ""name"": ""item"",
                        ""type"": ""release"",
                        ""enabled"": true,
                        ""impressionData"": true,
                        ""strategies"": [
                            {
                                ""name"": ""default"",
                                ""parameters"": {},
                                ""constraints"": [
                                    {
                                        ""contextName"": ""item-id"",
                                        ""operator"": ""NUM_EQ"",
                                        ""value"": ""1"",
                                        ""caseInsensitive"": false,
                                        ""inverted"": false
                                    }
                                ]
                            }]
                    }
                ]
            }";
            var unleash = CreateUnleash(appname, state);
            unleash.ConfigureEvents(cfg =>
            {
                cfg.ImpressionEvent = evt => { callbackEvent = evt; };
            });

            // Act
            var result = unleash.IsEnabled("item");
            unleash.Dispose();

            // Assert
            result.Should().BeTrue();
            callbackEvent.Should().NotBeNull();
            callbackEvent.Enabled.Should().BeTrue();
            callbackEvent.Context.AppName.Should().Be(appname);
            callbackEvent.Variant.Should().BeNull();
        }

        [Test]
        public void Impression_Event_Does_Not_Get_Called_When_Not_Opted_In()
        {
            // Arrange
            ImpressionEvent callbackEvent = null;
            var appname = "testapp";

            var state = @"
            {
                ""version"": 2,
                ""features"": [
                    {
                        ""name"": ""item"",
                        ""type"": ""release"",
                        ""enabled"": true,
                        ""impressionData"": false,
                        ""strategies"": [
                            {
                                ""name"": ""default"",
                                ""parameters"": {},
                                ""constraints"": [
                                    {
                                        ""contextName"": ""item-id"",
                                        ""operator"": ""NUM_EQ"",
                                        ""value"": ""1"",
                                        ""caseInsensitive"": false,
                                        ""inverted"": false
                                    }
                                ]
                            }]
                    }
                ]
            }";

            var unleash = CreateUnleash(appname, state);
            unleash.ConfigureEvents(cfg =>
            {
                cfg.ImpressionEvent = evt => { callbackEvent = evt; };
            });

            // Act
            var result = unleash.IsEnabled("item");
            unleash.Dispose();

            // Assert
            result.Should().BeTrue();
            callbackEvent.Should().BeNull();
        }

        [Test]
        public void Impression_Event_Callback_Invoker_Catches_Exception()
        {
            // Arrange
            var appname = "testapp";
            var state = @"
            {
                ""version"": 2,
                ""features"": [
                    {
                        ""name"": ""item"",
                        ""type"": ""release"",
                        ""enabled"": true,
                        ""impressionData"": true,
                        ""strategies"": [
                            {
                                ""name"": ""default"",
                                ""parameters"": {},
                                ""constraints"": [
                                    {
                                        ""contextName"": ""item-id"",
                                        ""operator"": ""NUM_EQ"",
                                        ""value"": ""1"",
                                        ""caseInsensitive"": false,
                                        ""inverted"": false
                                    }
                                ]
                            }]
                    }
                ]
            }";

            var unleash = CreateUnleash(appname, state);
            unleash.ConfigureEvents(cfg =>
            {
                cfg.ImpressionEvent = evt => { throw new Exception("Something bad just happened!"); };
            });

            // Act, Assert
            Assert.DoesNotThrow(() => { unleash.IsEnabled("item"); });
            unleash.Dispose();
        }

        [Test]
        public void Impression_Event_Callback_Null_Does_Not_Throw()
        {
            // Arrange
            var appname = "testapp";
            var state = @"
            {
                ""version"": 2,
                ""features"": [
                    {
                        ""name"": ""item"",
                        ""type"": ""release"",
                        ""enabled"": true,
                        ""impressionData"": true,
                        ""strategies"": [
                            {
                                ""name"": ""default"",
                                ""parameters"": {},
                                ""constraints"": [
                                    {
                                        ""contextName"": ""item-id"",
                                        ""operator"": ""NUM_EQ"",
                                        ""value"": ""1"",
                                        ""caseInsensitive"": false,
                                        ""inverted"": false
                                    }
                                ]
                            }]
                    }
                ]
            }";
            var unleash = CreateUnleash(appname, "");//state);
            unleash.ConfigureEvents(cfg =>
            {
                cfg.ImpressionEvent = null;
            });

            // Act, Assert
            Assert.DoesNotThrow(() => { unleash.IsEnabled("item"); });
            unleash.Dispose();
        }

        [Test]
        public void Impression_Event_Gets_Called_For_Variants()
        {
            // Arrange
            ImpressionEvent callbackEvent = null;
            var appname = "testapp";

            var state = @"
            {
                ""version"": 2,
                ""features"": [
                    {
                        ""name"": ""item"",
                        ""type"": ""release"",
                        ""enabled"": true,
                        ""impressionData"": true,
                        ""strategies"": [
                            {
                                ""name"": ""default"",
                                ""parameters"": {},
                                ""constraints"": [
                                    {
                                        ""contextName"": ""item-id"",
                                        ""operator"": ""NUM_EQ"",
                                        ""value"": ""1"",
                                        ""caseInsensitive"": false,
                                        ""inverted"": false
                                    }
                                ]
                            }],
                        ""variants"": [
                            {
                                ""name"": ""blue"",
                                ""weight"": 100,
                                ""payload"": {
                                    ""type"": ""string"",
                                    ""value"": ""val1""
                                }
                            }
                        ]
                    }
                ]
            }";


            var unleash = CreateUnleash(appname, state);
            unleash.ConfigureEvents(cfg =>
            {
                cfg.ImpressionEvent = evt => { callbackEvent = evt; };
            });

            // Act
            var result = unleash.GetVariant("item");
            unleash.Dispose();

            // Assert
            result.Name.Should().Be("blue");
            callbackEvent.Should().NotBeNull();
            callbackEvent.Enabled.Should().BeTrue();
            callbackEvent.Variant.Should().Be("blue");
            callbackEvent.Context.AppName.Should().Be(appname);
        }

        [Test]
        public void Unhooked_Impression_Events_Doesnt_Cause_Everything_To_Fail()
        {
            // Arrange
            var appname = "testapp";

            var state = @"
            {
                ""version"": 2,
                ""features"": [
                    {
                        ""name"": ""yup"",
                        ""type"": ""release"",
                        ""enabled"": true,
                        ""impressionData"": false,
                        ""strategies"": [
                            {
                                ""name"": ""default"",
                                ""parameters"": {},
                                ""constraints"": [
                                    {
                                        ""contextName"": ""item-id"",
                                        ""operator"": ""NUM_EQ"",
                                        ""value"": ""1"",
                                        ""caseInsensitive"": false,
                                        ""inverted"": false
                                    }
                                ]
                            }]
                    }
                ]
            }";

            var unleash = CreateUnleash(appname, state);

            // Act
            var enabled = unleash.IsEnabled("yup");
            unleash.Dispose();

            // Assert
            enabled.Should().BeTrue();
        }

        public static IUnleash CreateUnleash(string name, string state)
        {
            var fakeHttpClientFactory = A.Fake<IHttpClientFactory>();
            var fakeHttpMessageHandler = new TestHttpMessageHandler(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(state, Encoding.UTF8, "application/json"),
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

            var contextBuilder = new UnleashContext.Builder();
            contextBuilder.AddProperty("item-id", "1");

            var settings = new UnleashSettings
            {
                AppName = name,
                UnleashContextProvider = new DefaultUnleashContextProvider(contextBuilder.Build()),
                HttpClientFactory = fakeHttpClientFactory,
                ScheduledTaskManager = fakeScheduler,
                UseYggdrasil = true
            };

            var unleash = new DefaultUnleash(settings);

            return unleash;
        }
    }
}
