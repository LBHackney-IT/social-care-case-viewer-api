namespace SocialCareCaseViewerApi.V1.Domain
{
    public class RelatedRelationship
    {
        public long Id { get; set; }
        public long PersonId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string IsMainCarer { get; set; }
        public string IsInformalCarer { get; set; }
        public string Details { get; set; }
    }
}
