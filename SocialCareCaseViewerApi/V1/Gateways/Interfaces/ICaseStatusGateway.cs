using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Infrastructure;
using CaseStatus = SocialCareCaseViewerApi.V1.Domain.CaseStatus;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Gateways.Interfaces
{
    public interface ICaseStatusGateway
    {
        List<CaseStatus> GetCaseStatusesByPersonId(long personId);
        CaseStatus? GetCaseStatusesByPersonIdDate(long personId, System.DateTime period);
        CaseStatus CreateCaseStatus(CreateCaseStatusRequest request);
        CaseStatus? GetCasesStatusByCaseStatusId(long id);
        CaseStatus UpdateCaseStatus(UpdateCaseStatusRequest request);
        CaseStatus CreateCaseStatusAnswer(CreateCaseStatusAnswerRequest request);
    }
}
