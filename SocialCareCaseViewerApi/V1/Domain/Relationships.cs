namespace SocialCareCaseViewerApi.V1.Domain
{
    public class Relationships
    {
        public long PersonId { get; set; }

        public PersonalRelationships<long> PersonalRelationships { get; set; }
    }
}
