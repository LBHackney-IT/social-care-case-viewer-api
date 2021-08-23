using System.Collections.Generic;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Domain
{
    public class CaseStatus
    {
        public long Id { get; set; }
        public string? Type { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public string? Notes { get; set; }
        public List<CaseStatusField> Fields { get; set; } = new List<CaseStatusField>();
    }
}
