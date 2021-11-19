using System;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Domain
{
    public class CaseStatusAnswer
    {
        public string? Option { get; set; }
        public string? Value { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? DiscardedAt { get; set; }
        public string GroupId { get; set; } = null!;
        public DateTime? EndDate { get; set; }
    }
}
