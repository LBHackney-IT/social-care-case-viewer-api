using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.V1.Gateways
{
    public interface ICaseStatusGateway
    {
        List<CaseStatus> GetCaseStatusesByPersonId(long personId);
        CaseStatus GetCaseStatusesByPersonIdDate(long personId, System.DateTime period);
        Infrastructure.CaseStatusType GetCaseStatusTypeWithFields(string caseStatusType);
        CaseStatus CreateCaseStatus(CreateCaseStatusRequest request);
    }
}
