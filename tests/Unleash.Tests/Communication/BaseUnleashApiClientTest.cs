﻿using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Unleash.Communication;
using Unleash.Serialization;

namespace Unleash.Tests.Communication
{
    public abstract class BaseUnleashApiClientTest
    {
        private static IUnleashApiClient CreateApiClient()
        {
            var apiUri = new Uri("http://unleash.herokuapp.com/api/");

            var jsonSerializer = new DynamicNewtonsoftJsonSerializer();
            jsonSerializer.TryLoad();

            var httpClientFactory = new DefaultHttpClientFactory();

            var requestHeaders = new UnleashApiClientRequestHeaders
            {
                AppName = "api-test-client",
                InstanceTag = "instance1",
                CustomHttpHeaders = new Dictionary<string, string>()
                {
                    // "Test" token from 21.10.2021
                    { "Authorization", "*:default.77c45b703a681983b714fee87e575a823bfb1fd0ab282d9399647243" }
                },
                CustomHttpHeaderProvider = null
            };

            var httpClient = httpClientFactory.Create(apiUri);
            var client = new UnleashApiClient(httpClient, jsonSerializer, requestHeaders);
            return client;
        }

        internal IUnleashApiClient api
        {
            get => TestExecutionContext.CurrentContext.CurrentTest.Properties.Get("api") as IUnleashApiClient;
            set => TestExecutionContext.CurrentContext.CurrentTest.Properties.Set("api", value);
        }

        [SetUp]
        public void SetupTest()
        {
            api = CreateApiClient();
        }
    }
}