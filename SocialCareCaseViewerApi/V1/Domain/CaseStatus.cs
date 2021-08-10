using System;
using SocialCareCaseViewerApi.V1.Infrastructure;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Domain
{
    public class CaseStatus
    {
        public string Type { get; set; } = null!;
        public string SubType { get; set; } = null!;
        public string StartDate { get; set; } = null!;
        public string? EndDate { get; set; }
        public string? Notes { get; set; }
    }
}
