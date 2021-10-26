using SocialCareCaseViewerApi.V1.Boundary.Requests;
using System;
using System.Collections.Generic;
using CaseStatus = SocialCareCaseViewerApi.V1.Domain.CaseStatus;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Gateways.Interfaces
{
    public interface ICaseStatusGateway
    {
        List<CaseStatus> GetActiveCaseStatusesByPersonId(long personId);
        List<CaseStatus> GetCaseStatusesByPersonId(long personId);
        List<CaseStatus> GetClosedCaseStatusesByPersonIdAndDate(long personId, DateTime startDate);
        CaseStatus CreateCaseStatus(CreateCaseStatusRequest request);
        CaseStatus? GetCasesStatusByCaseStatusId(long id);
        CaseStatus UpdateCaseStatus(UpdateCaseStatusRequest request);
        CaseStatus CreateCaseStatusAnswer(CreateCaseStatusAnswerRequest request);
        CaseStatus ReplaceCaseStatusAnswer(CreateCaseStatusAnswerRequest request);
    }
}
