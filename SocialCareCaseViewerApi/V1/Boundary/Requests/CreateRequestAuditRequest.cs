using System.Collections.Generic;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class CreateRequestAuditRequest
    {
        public string UserName { get; set; }

        public string ActionName { get; set; }

        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }
}
