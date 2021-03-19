using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class ListVisitsResponse
    {
        public List<Visit> Visits { get; set; }
    }
}
