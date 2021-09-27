using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Domain;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class CaseStatusResponse
    {
        public long Id { get; set; }
        public string Type { get; set; }
        public string StartDate { get; set; }
        public string? EndDate { get; set; }
        public string? Notes { get; set; }
        public List<CaseStatusField> Fields { get; set; }
    }
}
