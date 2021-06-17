namespace SocialCareCaseViewerApi.V1.Domain
{
    public class RelationshipsV1
    {
        public long PersonId { get; set; }

        public PersonalRelationships<long> PersonalRelationships { get; set; }
    }
}
