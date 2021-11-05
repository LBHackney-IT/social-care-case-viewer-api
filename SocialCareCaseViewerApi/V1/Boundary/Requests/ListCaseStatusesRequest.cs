using Microsoft.AspNetCore.Mvc;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class ListCaseStatusesRequest
    {
        [FromRoute(Name = "personId")]
        public long PersonId { get; set; }

        [FromQuery(Name = "include_closed_cases")]

        public bool IncludeClosedCases { get; set; } = false;
    }
}
