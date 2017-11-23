namespace Unleash
{
    using System.Collections.Generic;

    public class UnleashContext
    {
        public UnleashContext(string userId, string sessionId, string remoteAddress, Dictionary<string, string> properties)
        {
            UserId = userId;
            SessionId = sessionId;
            RemoteAddress = remoteAddress;
            Properties = properties;
        }

        public string UserId { get; }
        public string SessionId { get; }
        public string RemoteAddress { get; }
        public Dictionary<string, string> Properties { get; }

        public static Builder New()
        {
            return new Builder();
        }

        public class Builder
        {
            private string userId;
            private string sessionId;
            private string remoteAddress;
            private readonly Dictionary<string, string> properties = new Dictionary<string, string>();

            public Builder UserId(string userId)
            {
                this.userId = userId;
                return this;
            }

            public Builder SessionId(string sessionId)
            {
                this.sessionId = sessionId;
                return this;
            }

            public Builder RemoteAddress(string remoteAddress)
            {
                this.remoteAddress = remoteAddress;
                return this;
            }

            public Builder AddProperty(string name, string value)
            {
                properties.Add(name, value);
                return this;
            }

            public UnleashContext Build()
            {
                return new UnleashContext(userId, sessionId, remoteAddress, properties);
            }
        }
    }
}