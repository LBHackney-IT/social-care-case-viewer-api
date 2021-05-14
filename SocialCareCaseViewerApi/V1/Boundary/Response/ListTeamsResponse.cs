using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class ListTeamsResponse
    {
        public IList<TeamResponse> Teams { get; set; }
    }
}
