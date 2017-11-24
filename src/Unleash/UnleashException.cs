namespace Unleash
{
    using System;
    using System.Runtime.Serialization;

    /// <inheritdoc />
    [Serializable]
    public class UnleashException : Exception
    {
        /// <inheritdoc />
        public UnleashException()
        {
        }

        /// <inheritdoc />
        public UnleashException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public UnleashException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <inheritdoc />
        protected UnleashException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}