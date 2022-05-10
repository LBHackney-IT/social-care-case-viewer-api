using System;
using System.Runtime.Serialization;

namespace AlertsHandlerLambda
{
    public class GoogleApiException : Exception
    {
        public GoogleApiException(string message) : base(message) { }
    }

    public class ConfigurationException : Exception
    {
        public ConfigurationException(string message) : base(message) { }
    }
}
