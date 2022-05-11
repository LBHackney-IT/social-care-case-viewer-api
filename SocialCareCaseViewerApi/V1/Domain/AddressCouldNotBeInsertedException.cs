using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace SocialCareCaseViewerApi.V1.Domain
{
    [Serializable]
    public class AddressCouldNotBeInsertedException : Exception
    {
        protected AddressCouldNotBeInsertedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public AddressCouldNotBeInsertedException(SerializationInfo? info, StreamingContext? context, string message) : base(message) { }
    }
}
