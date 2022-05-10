using System;
using System.Runtime.Serialization;

namespace AlertsHandlerLambda
{
    [Serializable]
    public class GoogleApiException : Exception
    {
        protected GoogleApiException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public GoogleApiException(SerializationInfo? info, StreamingContext? context, string message) : base(message) { }
    }

    [Serializable]
    public class ConfigurationException : Exception
    {
        protected ConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public ConfigurationException(SerializationInfo? info, StreamingContext? context, string message) : base(message) { }
    }
}
