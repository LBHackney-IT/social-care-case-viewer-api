using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class GetCaseStatusFieldsResponse
    {
        public IEnumerable<CaseStatusTypeField> Fields;
    }
}
