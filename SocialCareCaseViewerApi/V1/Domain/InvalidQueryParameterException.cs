using System;
using System.Runtime.Serialization;

namespace SocialCareCaseViewerApi.V1.Domain
{
    [Serializable]
    public class InvalidQueryParameterException : Exception
    {
        protected InvalidQueryParameterException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public InvalidQueryParameterException(string message) : base(message) { }
    }
}
