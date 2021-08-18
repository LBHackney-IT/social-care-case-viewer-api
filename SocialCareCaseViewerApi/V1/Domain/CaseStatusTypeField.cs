using System.Collections.Generic;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Domain
{
    public class CaseStatusTypeField
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<CaseStatusTypeFieldOption>? Options { get; set; }
    }
}
