using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class ListRelationshipsV1Response
    {
        public long PersonId { get; set; }

        public PersonalRelationships<RelatedPerson> PersonalRelationships { get; set; } = new PersonalRelationships<RelatedPerson>();
    }
}
