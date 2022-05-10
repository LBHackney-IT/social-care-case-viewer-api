using System;
using System.Runtime.Serialization;

namespace AlertsHandlerLambda
{
    [Serializable]
    public class GoogleApiException : Exception
    {
        public GoogleApiException(SerializationInfo info, StreamingContext context, String message) : base(message) { }
    }

    [Serializable]
    public class ConfigurationException : Exception
    {
        public ConfigurationException(string message) : base(message) { }
    }
}
