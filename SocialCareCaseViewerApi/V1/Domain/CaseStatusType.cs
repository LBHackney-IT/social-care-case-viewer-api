#nullable enable
using System.Collections.Generic;

namespace SocialCareCaseViewerApi.V1.Domain
{
    public class CaseStatusType
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<CaseStatusTypeField> Fields { get; set; } = new List<CaseStatusTypeField>();
    }
}
