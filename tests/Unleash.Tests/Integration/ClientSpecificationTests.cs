using FakeItEasy;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unleash.Scheduling;
using Unleash.Tests.Mock;

namespace Unleash.Tests.Specifications
{
    public class ClientSpecificationTests
    {
        [TestCaseSource(typeof(TestFactory), "Tests")]
        public void ShouldBeEnabledWhenExpected(Action testAction)
        {
            testAction();
        }
    }

    internal class TestFactory
    {
        public static TestCaseData[] Tests { get; private set; } 

        static TestFactory()
        {
            var specificationsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..", "..", "Integration", "Data");

            var testDefinitions = ParseJsonFile<string[]>(Path.Combine(specificationsPath, "index.json"))
                .Select(specificationsFile => ParseJsonFile<TestDefinition>(Path.Combine(specificationsPath, specificationsFile)))
                .ToList();

            Tests = testDefinitions.SelectMany(testDefinition =>
                {
                    var tests = testDefinition.Tests?.Select(test =>
                    {
                        var testCaseData = new TestCaseData(CreateTestAction(testDefinition, test));
                        
                        testCaseData.SetName($"{testDefinition.Name}.{test.Description.Replace(" ", "_").Replace(".", "_")}");

                        return testCaseData;
                    });

                    var variantTests = testDefinition.VariantTests?.Select(test =>
                    {
                        var testCaseData = new TestCaseData(CreateVariantTestAction(testDefinition, test));

                        testCaseData.SetName($"{testDefinition.Name}.{test.Description.Replace(" ", "_").Replace(".", "_")}");

                        return testCaseData;
                    });

                    if (tests == null) tests = new List<TestCaseData>();
                    if (variantTests == null) variantTests = new List<TestCaseData>();

                    return tests.Union(variantTests);

                })
                .ToArray();
        }

        private static T ParseJsonFile<T>(string filePath)
        {
            var body = File.ReadAllText(filePath, Encoding.UTF8);
            return JsonConvert.DeserializeObject<T>(body);
        }

        private static Action CreateTestAction(TestDefinition testDefinition, TestCase testCase)
        {
            return () =>
            {
                // Arrange
                var unleash = CreateUnleash(testDefinition, testCase.Context);

                // Act
                var result = unleash.IsEnabled(testCase.ToggleName);

                // Assert
                Assert.AreEqual(testCase.ExpectedResult, result, testCase.Description);
            };
        }

        private static Action CreateVariantTestAction(TestDefinition testDefinition, TestCaseVariant testCase)
        {
            return () =>
            {
                // Arrange
                var unleash = CreateUnleash(testDefinition, testCase.Context);

                // Act
                var result = unleash.GetVariant(testCase.ToggleName);

                // Assert
                Assert.AreEqual(testCase.ExpectedResult.Name, result.Name, testCase.Description);
                Assert.AreEqual(testCase.ExpectedResult.IsEnabled, result.IsEnabled, testCase.Description);
                Assert.AreEqual(testCase.ExpectedResult.Payload, result.Payload, testCase.Description);
            };  
        }

        public static IUnleash CreateUnleash(TestDefinition testDefinition, UnleashContextDefinition contextDefinition)
        {
            var fakeHttpClientFactory = A.Fake<IHttpClientFactory>();
            var fakeHttpMessageHandler = new TestHttpMessageHandler();
            var httpClient = new HttpClient(fakeHttpMessageHandler) { BaseAddress = new Uri("http://localhost") };
            var fakeScheduler = A.Fake<IUnleashScheduledTaskManager>();
            var fakeFileSystem = new MockFileSystem();

            A.CallTo(() => fakeHttpClientFactory.Create(A<Uri>._)).Returns(httpClient);
            A.CallTo(() => fakeScheduler.Configure(A<IEnumerable<IUnleashScheduledTask>>._, A<CancellationToken>._)).Invokes(action =>
            {
                var task = ((IEnumerable<IUnleashScheduledTask>)action.Arguments[0]).First();
                task.ExecuteAsync((CancellationToken)action.Arguments[1]).Wait();
            });

            fakeHttpMessageHandler.Response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(testDefinition.State.ToString(), Encoding.UTF8, "application/json"),
                Headers =
                {
                    ETag = new EntityTagHeaderValue("\"123\"")
                }
            };

            var contextBuilder = new UnleashContext.Builder()
                .UserId(contextDefinition.UserId)
                .SessionId(contextDefinition.SessionId)
                .RemoteAddress(contextDefinition.RemoteAddress)
                .Environment(contextDefinition.Environment)
                .AppName(contextDefinition.AppName);
                    
            if (contextDefinition.Properties != null)
            {
                foreach (var property in contextDefinition.Properties)
                {
                    contextBuilder.AddProperty(property.Key, property.Value);
                }
            }

            var settings = new UnleashSettings
            {
                AppName = testDefinition.Name,
                UnleashContextProvider = new DefaultUnleashContextProvider(contextBuilder.Build()),
                HttpClientFactory = fakeHttpClientFactory,
                ScheduledTaskManager = fakeScheduler,
                FileSystem = fakeFileSystem
            };

            var unleash = new DefaultUnleash(settings);

            return unleash;
        }

        internal class TestHttpMessageHandler : HttpMessageHandler
        {
            public HttpResponseMessage Response { get; set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(Response);
            }
        }
    }
}