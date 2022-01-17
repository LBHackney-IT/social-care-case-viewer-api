using Microsoft.AspNetCore.Mvc;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
#nullable enable
    public class QueryMashReferrals
    {
        [FromQuery(Name = "id")]
        public long? Id { get; set; }

        [FromQuery(Name = "WorkerEmail")]
        public long? WorkerEmail { get; set; }
    }
}
