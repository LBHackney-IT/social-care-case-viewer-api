using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class ListRelationshipsV1Response
    {
        public long PersonId { get; set; }

        public PersonalRelationshipsV1<RelatedPersonV1> PersonalRelationships { get; set; } = new PersonalRelationshipsV1<RelatedPersonV1>();
    }
}
