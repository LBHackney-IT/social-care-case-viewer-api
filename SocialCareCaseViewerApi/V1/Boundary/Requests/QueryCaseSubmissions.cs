using System.Collections.Generic;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
#nullable enable
{
    public class QueryCaseSubmissions
    {
        public string? FormId { get; set; }

        public IEnumerable<string>? SubmissionStates { get; set; }
    }
}
