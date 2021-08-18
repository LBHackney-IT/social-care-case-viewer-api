using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class GetCaseStatusFieldsResponse
    {
        public List<CaseStatusTypeField> Fields { get; set; } = new List<CaseStatusTypeField>();
    }
}
