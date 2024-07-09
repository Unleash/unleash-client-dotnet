using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using Unleash.ClientFactory;
using Unleash.Scheduling;
using static Unleash.Tests.Specifications.TestFactory;

namespace Unleash.Tests.Variants
{
    public class VariantsTests
    {
        [Test]
        public void Sets_FeatureEnabled_True_When_Variant_Found()
        {
            // Arrange
            var unleash = GetUnleash();

            // Act
            var variant = unleash.GetVariant("enabled.with.variants");

            // Assert
            variant.IsEnabled.Should().BeTrue();
            variant.FeatureEnabled.Should().BeTrue();
            new[] { "A", "B" }.Should().Contain(variant.Name);
        }

        [Test]
        public void Sets_FeatureEnabled_True_When_Variant_Not_Found()
        {
            // Arrange
            var unleash = GetUnleash();

            // Act
            var variant = unleash.GetVariant("enabled.no.variants");

            // Assert
            variant.IsEnabled.Should().BeFalse();
            variant.FeatureEnabled.Should().BeTrue();
        }

        [Test]
        public void Sets_FeatureEnabled_False_When_Feature_Missing()
        {
            // Arrange
            var unleash = GetUnleash();

            // Act
            var variant = unleash.GetVariant("missing");

            // Assert
            variant.IsEnabled.Should().BeFalse();
            variant.FeatureEnabled.Should().BeFalse();
        }

        private IUnleash GetUnleash()
        {
            var fakeHttpClientFactory = A.Fake<IHttpClientFactory>();
            var settings = new UnleashSettings()
            {
                AppName = "testapp",
                UnleashApi = new Uri("http://localhost:8080/"),
                ScheduledTaskManager = A.Fake<IUnleashScheduledTaskManager>(),
                HttpClientFactory = fakeHttpClientFactory
            };
            var responseContent = TestData;
            var fakeHttpMessageHandler = new TestHttpMessageHandler();
            fakeHttpMessageHandler.Response = new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json"),
                Headers =
                {
                    ETag = new EntityTagHeaderValue("\"123\"")
                }
            };
            var client = new HttpClient(fakeHttpMessageHandler);
            client.BaseAddress = settings.UnleashApi;
            var factory = new UnleashClientFactory();
            A.CallTo(() => fakeHttpClientFactory.Create(A<Uri>._)).Returns(client);
            return factory.CreateClient(settings, synchronousInitialization: true);
        }

        public string TestData => @"{
            ""version"": 2,
            ""features"": [
                {
                    ""name"": ""enabled.with.variants"",
                    ""description"": ""Test"",
                    ""enabled"": true,
                    ""strategies"": [
                        {
                            ""name"": ""flexibleRollout"",
                            ""parameters"": {
                                ""rollout"": ""100"",
                                ""stickiness"": ""default"",
                                ""groupId"": ""default"",
                            }
                        }
                    ],
                    ""variants"": [
                        {
                            ""name"": ""A"",
                            ""weight"": 50
                        },
                        {
                            ""name"": ""B"",
                            ""weight"": 50
                        }
                    ]
                },
                {
                    ""name"": ""enabled.no.variants"",
                    ""description"": ""Test"",
                    ""enabled"": true,
                    ""strategies"": [
                        {
                            ""name"": ""default"",
                        }
                    ]
                }
            ]
        }";
    }

}