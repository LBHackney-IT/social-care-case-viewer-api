using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class ListCaseStatusesResponse
    {
        public long PersonId { get; set; }
        public List<CaseStatus> CaseStatuses { get; set; }
    }
}
