using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class ListRelationshipsV1Response
    {
        public long PersonId { get; set; }

        public PersonalRelationshipsV1<RelatedPerson> PersonalRelationships { get; set; } = new PersonalRelationshipsV1<RelatedPerson>();
    }
}
