namespace SocialCareCaseViewerApi.V1.Domain
{
    public class RelationshipsV1
    {
        public long PersonId { get; set; }

        public PersonalRelationshipsV1<long> PersonalRelationships { get; set; }
    }
}
