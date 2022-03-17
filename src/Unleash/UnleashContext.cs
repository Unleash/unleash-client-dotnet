namespace Unleash
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A context which the feature request should be validated againt. Usually scoped to a web request through an implementation of IUnleashContextProvider.
    /// </summary>
    public class UnleashContext
    {
        public string AppName { get; set; }
        public string Environment { get; set; }
        public string UserId { get; set; }
        public string SessionId { get; set; }
        public string RemoteAddress { get; set; }
        public DateTimeOffset? CurrentTime { get; set; }
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        public string GetByName(string contextName)
        {
            switch (contextName)
            {
                case "environment": 
                    return Environment;
                case "appName": 
                    return AppName;
                case "userId": 
                    return UserId;
                case "sessionId": 
                    return SessionId;
                case "remoteAddress":
                    return RemoteAddress;
                default:
                    string result;
                    Properties.TryGetValue(contextName, out result);
                    return result;
            }
        }

        public UnleashContext ApplyStaticFields(UnleashSettings settings)
        {
            var builder = new Builder(this);

            if (string.IsNullOrEmpty(Environment))
            {
                builder.Environment(settings.Environment);
            }

            if (string.IsNullOrEmpty(AppName))
            {
                builder.AppName(settings.AppName);
            }

            return builder.Build();
        }

        internal static Builder New()
        {
            return new Builder();
        }

        internal class Builder
        {
            private string appName;
            private string environment;
            private string userId;
            private string sessionId;
            private string remoteAddress;
            private DateTimeOffset? currentTime;
            private readonly Dictionary<string, string> properties = new Dictionary<string, string>();

            public Builder()
            {

            }

            public Builder(UnleashContext context)
            {
                appName = context.AppName;
                environment = context.Environment;
                userId = context.UserId;
                sessionId = context.SessionId;
                remoteAddress = context.RemoteAddress;
                currentTime = context.CurrentTime;
                properties = context.Properties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }

            public Builder AppName(string appName)
            {
                this.appName = appName;
                return this;
            }

            public Builder Environment(string environment)
            {
                this.environment = environment;
                return this;
            }

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

            public Builder CurrentTime(DateTimeOffset currentTime)
            {
                this.currentTime = currentTime;
                return this;
            }

            public Builder Now()
            {
                this.currentTime = DateTimeOffset.UtcNow;
                return this;
            }

            public Builder AddProperty(string name, string value)
            {
                properties.Add(name, value);
                return this;
            }

            public UnleashContext Build()
            {
                return new UnleashContext()
                {
                    AppName = appName,
                    Environment = environment,
                    UserId = userId,
                    SessionId = sessionId,
                    RemoteAddress = remoteAddress,
                    Properties = properties,
                    CurrentTime = currentTime
                };
            }
        }
    }
}