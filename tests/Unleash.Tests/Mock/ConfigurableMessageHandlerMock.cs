﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Unleash.Tests.Mock
{
    public class ConfigurableMessageHandlerMock : HttpMessageHandler
    {
        private Dictionary<string, HttpResponseMessage> configuredResponses = new Dictionary<string, HttpResponseMessage>();
        public List<HttpRequestMessage> SentMessages = new List<HttpRequestMessage>();


        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            SentMessages.Add(request);

            return Task.FromResult(configuredResponses[request.RequestUri.ToString()]);
        }

        public void Configure(string path, HttpResponseMessage response)
        {
            configuredResponses.Add(path, response);
        }
    }
}
