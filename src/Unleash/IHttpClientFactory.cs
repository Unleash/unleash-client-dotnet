using System;
using System.Net.Http;

namespace Unleash
{
    /// <summary>
    /// Factory for creating HttpClient used to communicate with Unleash Server api.
    /// </summary>
    public interface IHttpClientFactory
    {
        /// <summary>
        /// Called during background task initialization.
        /// </summary>
        HttpClient Create(Uri unleashApiUri);
    }
}
