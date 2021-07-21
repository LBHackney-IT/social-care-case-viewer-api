using System.Collections.Generic;

namespace SocialCareCaseViewerApi.V1.Domain
{
    public class PersonalRelationship
    {
        public string Type { get; set; }
        public List<RelatedRelationship> Relationships { get; set; } = new List<RelatedRelationship>();
    }
}
