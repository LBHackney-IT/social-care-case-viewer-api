using System;

namespace AlertsHandlerLambda
{
    [Serializable]
    public class GoogleApiException : Exception
    {
        public GoogleApiException(string message) : base(message) { }
    }

    [Serializable]
    public class ConfigurationException : Exception
    {
        public ConfigurationException(string message) : base(message) { }
    }
}
