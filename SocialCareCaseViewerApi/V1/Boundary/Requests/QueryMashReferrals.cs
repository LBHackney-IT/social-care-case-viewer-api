using Microsoft.AspNetCore.Mvc;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    #nullable enable
    public class QueryMashReferrals
    {
        [FromQuery(Name = "id")]
        public string? Id { get; set; }
    }
}
