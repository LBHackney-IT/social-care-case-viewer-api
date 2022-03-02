using System;

namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class ResidentTeamResponse
    {
        public int Id { get; set; }

        public string Summary { get; set; }

        public string RagRating { get; set; }

        public DateTime AllocationDate { get; set; }
    }
}
