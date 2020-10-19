using System;

namespace SocialCareCaseViewerApi.V1.Domain
{
    public class ResidentNotFoundException : Exception
    {
    }

    public class DocumentNotFoundException : Exception
    {
        public DocumentNotFoundException(string message) : base(message) { }
    }
}
