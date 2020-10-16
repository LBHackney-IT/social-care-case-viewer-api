using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialCareCaseViewerApi.V1.Domain
{
    public class ResidentCouldNotBeinsertedException : Exception
    {
        public ResidentCouldNotBeinsertedException(string message) : base(message) { }
    }
}
