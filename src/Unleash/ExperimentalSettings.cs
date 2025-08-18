using System;

namespace Unleash {
    public class ExperimentalSettings
    {
        public void UseStreaming(Uri apiUri)
        {
            this.EnableStreaming = true;
            this.StreamingUri = apiUri;
        }

        internal bool EnableStreaming { get; private set; }
        internal Uri StreamingUri { get; private set; }
    }
}