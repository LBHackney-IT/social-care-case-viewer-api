using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class ListRelationshipsResponse
    {
        public long PersonId { get; set; }
        public List<PersonalRelationship> PersonalRelationships { get; set; } = new List<PersonalRelationship>();
    }
}
