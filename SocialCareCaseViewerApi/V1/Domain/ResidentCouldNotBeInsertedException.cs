using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace SocialCareCaseViewerApi.V1.Domain
{
    [Serializable]
    public class ResidentCouldNotBeInsertedException : Exception
    {
        protected ResidentCouldNotBeInsertedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public ResidentCouldNotBeInsertedException(string message) : base(message) { }
    }
}
