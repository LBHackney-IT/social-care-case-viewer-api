using System;
using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.V1.UseCase.Interfaces
{
    public interface IFormSubmissionsUseCase
    {
        (CaseSubmissionResponse, CaseSubmission) ExecutePost(CreateCaseSubmissionRequest request);

        CaseSubmissionResponse ExecuteGetById(string submissionId);

        List<CaseSubmissionResponse> ExecuteListBySubmissionStatus(SubmissionState state);

        void ExecuteFinishSubmission(string submissionId, FinishCaseSubmissionRequest request);
        CaseSubmissionResponse UpdateAnswers(string submissionId, string stepId,
            UpdateFormSubmissionAnswersRequest request);

    }
}
