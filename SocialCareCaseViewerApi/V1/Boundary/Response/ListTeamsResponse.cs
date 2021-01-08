using SocialCareCaseViewerApi.V1.Domain;
using System.Collections.Generic;

namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class ListTeamsResponse
    {
        public IList<Team> Teams { get; set; }
    }
}
