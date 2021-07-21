#nullable enable

namespace SocialCareCaseViewerApi.V1.Domain
{
    public class RelatedRelationship
    {
        public long Id { get; set; }
        public long PersonId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Gender { get; set; } = null!;
        public string IsMainCarer { get; set; } = null!;
        public string IsInformalCarer { get; set; } = null!;
        public string? Details { get; set; }
    }
}
