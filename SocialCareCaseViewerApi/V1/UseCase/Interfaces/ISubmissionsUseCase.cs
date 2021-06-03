using System;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;

namespace SocialCareCaseViewerApi.V1.UseCase.Interfaces
{
    public interface ISubmissionsUseCase
    {
        CaseSubmissionResponse ExecutePost(CreateCaseSubmissionRequest request);

        CaseSubmissionResponse ExecuteGetById(Guid submissionId);
    }
}
