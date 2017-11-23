namespace Unleash
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class UnleashException : Exception
    {
        public UnleashException()
        {
        }

        public UnleashException(string message) : base(message)
        {
        }

        public UnleashException(string message, Exception inner) : base(message, inner)
        {
        }

        protected UnleashException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}