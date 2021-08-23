using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class GetCaseStatusFieldsResponse
    {
        public string Description { get; set; }
        public string Name { get; set; }
        public List<CaseStatusTypeField> Fields { get; set; } = new List<CaseStatusTypeField>();
    }
}
